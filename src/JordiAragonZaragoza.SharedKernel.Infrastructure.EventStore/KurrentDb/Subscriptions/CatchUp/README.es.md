# Suscripción de puesta al día a todos los flujos (`KurrentDbAllStreamSubscription`)

## Resumen

`KurrentDbAllStreamSubscription` implementa una **suscripción de puesta al día (catch-up subscription)** al flujo `$all` de KurrentDB. Lee todos los eventos escritos alguna vez en el almacén (fase de puesta al día) y luego continúa escuchando nuevos eventos en tiempo real, todo dentro de una única conexión continua.

La suscripción está diseñada para gestionar **proyectores de modelos de lectura**: distribuye cada evento a un bus de eventos interno, que se ramifica a proyectores registrados que actualizan los modelos de lectura de PostgreSQL. Se persiste un punto de control (checkpoint) después de cada evento procesado correctamente para que la suscripción pueda reanudarse desde la última posición conocida tras un reinicio.

### Diferencias clave con una Suscripción Persistente

| Aspecto | Puesta al día (esta) | Persistente |
| --- | --- | --- |
| Almacenamiento de puntos de control | Gestionado por la aplicación (PostgreSQL vía `IRepository<Checkpoint>`) | Gestionado por el servidor (grupo de KurrentDB) |
| Reintento en caso de fallo | Gestionado por la aplicación (worker con backoff) | El servidor reintenta vía `maxRetryCount` |
| Grupo de consumidores | No aplicable | Debe crearse en el lado del servidor |
| Confirmación (Ack) | Implícita (avance del punto de control) | `Ack` / `Nack` explícito por evento |
| Concurrencia | Consumidor único, secuencial | Múltiples consumidores en un grupo |
| Filtro | Lado del cliente vía `SubscriptionFilterOptions` | Filtro de grupo en el lado del servidor |
| Notificación de caída | `await foreach` se completa o lanza excepción | Callback `HandleDrop` |
| Adecuado para | Proyectores, constructores de modelos de lectura | Relés de integración, fan-out multi-consumidor |

---

## Arquitectura interna

```
┌─────────────────────────────────────────────────────────────┐
│                  AllStreamSubscriptionBackgroundWorker       │
│  (BackgroundService — posee el bucle de reintento con backoff)      │
│                                                              │
│  ExecuteAsync                                                │
│    └── Task.Yield()          ← desbloquea el inicio del host        │
│    └── RunWithRetryAsync     ← bucle infinito de reintento          │
│          └── perform(ct)     ← llama a SubscribeToAllAsync    │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│               KurrentDbAllStreamSubscription                 │
│                                                              │
│  SubscribeToAllAsync                                         │
│    1. Leer punto de control de BD  (ámbito de inicio, corta duración)  │
│    2. kurrentDbClient.SubscribeToAll(fromPosition, ...)      │
│    3. await foreach(message in subscription.Messages)        │
│          └── HandleMessageAsync                              │
│                ├── StreamMessage.Event   → HandleEventAsync  │
│                ├── StreamMessage.CaughtUp → log              │
│                └── StreamMessage.FellBehind → log warning    │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼  ámbito por evento (corta duración)
┌─────────────────────────────────────────────────────────────┐
│  HandleEventAsync                                            │
│    1. SerializerHelper.Deserialize(resolvedEvent)            │
│    2. EventStoreActivityRestorer.RestoreFrom(metadata)       │
│    3. Resolver IUnitOfWork, IEventBus, IExecutionContextService│
│       IRepository<Checkpoint> — todos del mismo ámbito DI  │
│    4. executionContextService.OverrideExecutionContext(ctx)   │
│    5. unitOfWork.ExecuteInTransactionAsync(                  │
│         eventBus.PublishAsync(domainEvent)                   │
│         UpdateCheckpointAsync(resolvedEvent)                 │
│       )                                                      │
│    6. executionContextService.ClearExecutionContext()         │
└─────────────────────────────────────────────────────────────┘

```

### Por qué iterable en lugar de callback

El cliente de KurrentDB expone dos APIs de suscripción:

* **Iterable** (`SubscribeToAll` → `IAsyncEnumerable<StreamMessage>`): el consumidor controla el bucle con `await foreach`.
* **Callback** (`SubscribeToAllAsync` → registra delegados): el cliente controla el bucle internamente.

Esta implementación utiliza el patrón **iterable** intencionadamente:

* El control de flujo es explícito: si el procesamiento de eventos es lento, el productor espera naturalmente.
* El manejo de caídas es directo: el `await foreach` simplemente lanza una excepción o se completa, y el bucle de reintento en `AllStreamSubscriptionBackgroundWorker` maneja la reconexión.
* El procesamiento es estrictamente secuencial sin condiciones de carrera entre la caída y el manejo de eventos en vuelo (un peligro conocido en el modelo de callback cuando `HandleDrop` se dispara mientras `HandleEvent` aún se está ejecutando en otro hilo).
* Los seguimientos de pila (stack traces) son limpios y apuntan directamente al código de la aplicación.

El modelo de callback (como el utilizado por Eventuous) es más adecuado cuando se necesitan consumidores concurrentes con un `ConcurrencyLimit > 1`. Esta implementación procesa eventos secuencialmente, una transacción a la vez, que es el modelo correcto para proyectores de consumidor único.

---

## Ciclo de vida del Worker

```
Host.StartAsync()
  └── BackgroundService.StartAsync()
        └── ExecuteAsync()
              └── await Task.Yield()          ← devuelve el control al host inmediatamente
              └── RunWithRetryAsync(ct)
                    └── attempt = 0
                    └── loop:
                          ├── perform(ct)     ← bloquea hasta que la suscripción termina o lanza excepción
                          │     └── SubscribeToAllAsync()
                          │           ├── leer punto de control (ámbito de inicio)
                          │           ├── abrir flujo SubscribeToAll
                          │           ├── isSubscribed = 1 (Interlocked)
                          │           ├── await foreach(messages)
                          │           │     └── HandleEventAsync por evento
                          │           └── finally: isSubscribed = 0
                          │
                          ├── en retorno limpio → attempt=0, reconectar inmediatamente
                          ├── en OperationCanceledException (apagado del host) → retornar
                          └── en otra excepción → attempt++, esperar backoff, reintentar

Host.StopAsync()
  └── stoppingToken cancelado
        └── Task.Delay(delay, stoppingToken) lanza OperationCanceledException
              └── capturado por `when (stoppingToken.IsCancellationRequested)` → sale limpiamente

```

### `Task.Yield()` y el inicio del host

`BackgroundService.StartAsync` verifica si `ExecuteAsync` se completa de forma sincrónica. Si no lo hace, devuelve `Task.CompletedTask` inmediatamente para que el host continúe iniciando. El `await Task.Yield()` al principio de `ExecuteAsync` asegura que este camino siempre se tome, evitando que la reproducción del historial de puesta al día bloquee el inicio de otros servicios alojados.

---

## Manejo de errores

### Fallo en el procesamiento de eventos

Si `HandleEventAsync` lanza una excepción (error de proyección, tiempo de espera de BD, etc.), la excepción se propaga a través de `await foreach`, fuera de `SubscribeToAllAsync` y hacia `RunWithRetryAsync`. El bloque `finally` restablece `isSubscribed` a `0`. El bucle de reintento incrementa `attempt`, espera el retraso de backoff y vuelve a llamar a `SubscribeToAllAsync`. La suscripción se reanuda desde el **último punto de control confirmado**, por lo que el evento fallido se reintenta al reconectar.

La proyección y el checkpoint se confirman en la misma transacción PostgreSQL.
Si el proceso falla, PostgreSQL garantiza que ambos están confirmados o ninguno.
Al reiniciar, el checkpoint es autoritativo: el evento se reprocesa solo si la
transacción no se confirmó, en cuyo caso el read model también está en su estado
previo y el reprocesamiento es seguro.

**Los proyectores no necesitan ser idempotentes siempre que:**
- Solo produzcan efectos dentro de la transacción PostgreSQL (updates de read models).
- No realicen llamadas externas (HTTP, mensajes, emails) dentro del handler.

Si un proyector tiene efectos secundarios fuera de la transacción, esos efectos
pueden ocurrir más de una vez ante un fallo. En ese caso la idempotencia sí es
necesaria para ese projector específico.

### Fallo de deserialización (evento veneno)

Cuando `SerializerHelper.Deserialize` lanza una excepción (tipo de evento desconocido, JSON mal formado, desajuste de esquema):

* **`IgnoreDeserializationErrors = false` (predeterminado):** la excepción se propaga y la suscripción se detiene. El evento se reintenta al reconectar indefinidamente hasta que se resuelva el problema (tipo registrado, esquema corregido, evento saltado manualmente).
* **`IgnoreDeserializationErrors = true`:** el error se registra en el nivel `Error`, el punto de control se avanza más allá del evento veneno y el procesamiento continúa. **Úsese con precaución**: esto descarta silenciosamente eventos que no pudieron ser deserializados.

En ambos casos, el error se registra con el tipo de evento y la posición de confirmación para su trazabilidad.

### Caída de conexión / KurrentDB no disponible

Cuando `await foreach` termina debido a un error de red o reinicio del servidor:

| Intento | Retraso |
| --- | --- |
| 1 | ~2 s |
| 2 | ~4 s |
| 3 | ~8 s |
| 4 | ~16 s |
| 5 | ~32 s |
| 6+ | ~60 s (tope) |

Cada retraso tiene una fluctuación (jitter) aleatoria de ±20% para evitar el efecto de manada (thundering herd) cuando múltiples workers se reinician simultáneamente.

La fórmula de retraso es:

```
delay = min(InitialDelay × BackoffMultiplier^(attempt-1), MaxDelay) × (1 ± 0.2 jitter)

```

### Apagado del host durante el procesamiento

Si el host señala el apagado mientras se está procesando un evento:

1. El `cancellationToken` pasado a todas las operaciones asíncronas se cancela.
2. `Task.Delay`, `eventBus.PublishAsync`, las llamadas a `checkpointRepository`, etc., lanzan todas `OperationCanceledException`.
3. La excepción se propaga hasta `RunWithRetryAsync`.
4. La cláusula `catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)` la captura y sale limpiamente sin registrar un error.
5. El punto de control puede **no** haberse avanzado para el evento en vuelo. En el siguiente inicio, el evento se reprocesa desde el último punto de control confirmado. Los proyectores deben manejar la entrega duplicada.

---

## Configuración

### `KurrentDbAllStreamSubscriptionSettings` (vinculable desde `appsettings.json`)

| Propiedad | Tipo | Predeterminado | Descripción |
| --- | --- | --- | --- |
| `SubscriptionId` | `Guid` | `cbbaeb7e-...` | ID único usado como clave de punto de control en la base de datos. **Cámbielo si ejecuta múltiples suscripciones en la misma BD.** |
| `ResolveLinkTos` | `bool` | `false` | Si resolver eventos de enlace al evento original al que apuntan. |
| `IgnoreDeserializationErrors` | `bool` | `false` | Saltar eventos que no pueden ser deserializados en lugar de detener la suscripción. |

### `KurrentDbAllStreamSubscriptionOptions` (propiedades solo de código)

| Propiedad | Tipo | Predeterminado | Descripción |
| --- | --- | --- | --- |
| `FilterOptions` | `SubscriptionFilterOptions` | `ExcludeSystemEvents()` | Filtro de eventos del lado del servidor. Por defecto excluye eventos internos de KurrentDB (precedidos por `$`). |
| `Credentials` | `UserCredentials?` | `null` | Credenciales de anulación para esta suscripción. Si es `null`, se usan las credenciales por defecto del cliente. |
| `ConfigureOperation` | `Action<KurrentDBClientOperationOptions>?` | `null` | Configuración de operación gRPC de bajo nivel (deadline, política de reintento). |

`KurrentDbAllStreamSubscriptionOptions` no es directamente vinculable desde JSON porque `SubscriptionFilterOptions`, `UserCredentials` y `Action<>` no son serializables a JSON. La división en `Settings` (vinculable) y `Options` (tiempo de ejecución) es intencional.

---

## Registro DI

### Configuración predeterminada (no se necesita sección en `appsettings.json`)

```csharp
builder.Services.AddSharedKernelInfrastructureKurrentDbAllStreamSubscription();

```

Esto registra:

* `IOptions<KurrentDbAllStreamSubscriptionSettings>` vinculado a `"KurrentDb:AllStreamSubscription"` (recurre a los valores predeterminados de la clase si la sección está ausente).
* `KurrentDbAllStreamSubscription` como `Singleton`.
* `AllStreamSubscriptionBackgroundWorker` como `IHostedService`.

### Sección personalizada `appsettings.json`

```json
{
  "KurrentDb": {
    "AllStreamSubscription": {
      "SubscriptionId": "cbbaeb7e-a087-44cc-75a0-08dc80991837",
      "ResolveLinkTos": false,
      "IgnoreDeserializationErrors": false
    }
  }
}

```

Todos los campos son opcionales. Los campos omitidos mantienen sus valores predeterminados de clase.

### Opciones personalizadas solo de código (FilterOptions, Credentials)

Anule `FilterOptions` u otras propiedades no vinculables en la fábrica dentro de `AddSharedKernelInfrastructureKurrentDbAllStreamSubscription`:

```csharp
var options = new KurrentDbAllStreamSubscriptionOptions()
    .ApplySettings(settings);

// Anular propiedades no vinculables aquí:
options.FilterOptions = new SubscriptionFilterOptions(
    EventTypeFilter.Prefix("Cinema.", "Ticketing."),
    checkpointInterval: 1000);

options.Credentials = new UserCredentials("admin", "changeit");

```

### Ejecución de múltiples suscripciones

Cada suscripción debe tener un **`SubscriptionId` distinto** porque ese GUID se usa como clave primaria para la fila del punto de control. Si dos suscripciones comparten el mismo `SubscriptionId`, sobrescribirán los puntos de control de la otra y producirán resultados incorrectos.

Registre cada suscripción con su propia instancia de opciones y su propio `AllStreamSubscriptionBackgroundWorker`:

```csharp
// Suscripción A — proyecciones
services.AddHostedService(sp =>
{
    var sub = sp.GetRequiredService<KurrentDbAllStreamSubscriptionA>();
    var options = new KurrentDbAllStreamSubscriptionOptions { SubscriptionId = Guid.Parse("...A...") };
    return new AllStreamSubscriptionBackgroundWorker(
        sp.GetRequiredService<ILogger<AllStreamSubscriptionBackgroundWorker>>(),
        ct => sub.SubscribeToAllAsync(options, ct));
});

// Suscripción B — relé de integración
services.AddHostedService(sp =>
{
    var sub = sp.GetRequiredService<KurrentDbAllStreamSubscriptionB>();
    var options = new KurrentDbAllStreamSubscriptionOptions { SubscriptionId = Guid.Parse("...B...") };
    return new AllStreamSubscriptionBackgroundWorker(
        sp.GetRequiredService<ILogger<AllStreamSubscriptionBackgroundWorker>>(),
        ct => sub.SubscribeToAllAsync(options, ct));
});

```

---

## Ejemplo completo — Worker de proyector

```csharp
// Program.cs
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddInfrastructure();
builder.AddInfrastructureKurrentDbClient();   // registra KurrentDBClient vía Aspire

builder.Services
    .AddApplicationProjectors()               // registra implementaciones de IEventHandler
    .AddInfrastructureEntityFrameworkProjections(configuration, isDevelopment)
    .AddInfrastructureProjectionsRepositories();

builder.Services
    .AddSharedKernelApplicationProjectionsEventBus()
    .AddSharedKernelInfrastructureKurrentDbAllStreamSubscription()  // ← suscripción de puesta al día
    .AddSharedKernelInfrastructure()
    .AddSharedKernelInfrastructureProjections();

builder.AddInfrastructureEntityFrameworkProjections();

IHost app = builder.Build();
await app.RunAsync();

```

```json
// appsettings.Production.json
{
  "KurrentDb": {
    "AllStreamSubscription": {
      "SubscriptionId": "cbbaeb7e-a087-44cc-75a0-08dc80991837",
      "IgnoreDeserializationErrors": false
    }
  }
}

```

---

## Escalabilidad y compromisos

### Procesamiento secuencial

Los eventos se procesan estrictamente uno a la vez: `eventBus.PublishAsync` se completa, el punto de control se confirma, luego se lee el siguiente evento. Esto garantiza el orden y hace que los proyectores sean trivialmente idempotentes con respecto al orden, pero limita el rendimiento a una sola tubería de eventos.

**El rendimiento está limitado por:** `max_events_per_second ≈ 1 / (avg_event_processing_time_ms / 1000)`.

Si el rendimiento es insuficiente:

* Perfile dónde se invierte el tiempo (viajes de ida y vuelta a la BD, lógica del manejador de eventos).
* Considere suscripciones particionadas (requiere pasar al modelo de callback con `ConcurrencyLimit > 1`, similar al `PartitioningFilter` de Eventuous).
* Considere dividir el flujo de eventos por tipo y ejecutar suscripciones separadas para diferentes proyectores.

### Consumidor único

No hay grupo de consumidores. Solo una instancia de este worker debe procesar el mismo `SubscriptionId` a la vez. Ejecutar dos instancias con el mismo `SubscriptionId` causa una **carrera de puntos de control**: ambas instancias avanzan el punto de control concurrentemente, pudiendo saltar eventos o procesarlos dos veces sin detección.

En Kubernetes, asegúrese de tener `replicas: 1` para el despliegue del proyector, o use elección de líder.

### Granularidad del punto de control

El punto de control se confirma después de **cada** evento procesado correctamente, dentro de la misma transacción que la actualización de la proyección. Esto significa:

* Al reiniciar, como máximo se puede reprocesar un evento (el que estaba en vuelo al apagarse).
* No hay optimización de confirmación por lotes. Para flujos de alto volumen esto añade una escritura en BD por evento. Si esto se convierte en un cuello de botella, considere confirmar el punto de control cada N eventos (introduciendo una ventana de posible reprocesamiento al reiniciar).

### `IgnoreDeserializationErrors`

Configurarlo como `true` en producción significa que el proyector **salta silenciosamente** los eventos que no puede entender. Esto puede dejar los modelos de lectura permanentemente incompletos. Prefiera `false` en producción y corrija la causa raíz (registre el tipo de evento, actualice el esquema). Use `true` solo en desarrollo o para migraciones controladas donde los eventos omitidos se sabe que son irrelevantes.

---

## Casos límite

### Primera ejecución (sin punto de control)

En el primer inicio, no existe una fila de punto de control en la base de datos. La suscripción comienza desde `FromAll.Start`, reproduciendo todo el flujo `$all` desde la posición 0. Para almacenes grandes, esta fase de puesta al día puede tardar de minutos a horas. Durante este tiempo no se ha recibido todavía `StreamMessage.CaughtUp` y la suscripción está en modo de puesta al día.

### KurrentDB no disponible al inicio

Si KurrentDB no es alcanzable cuando el worker inicia, `kurrentDbClient.SubscribeToAll(...)` lanza una excepción. El bucle de reintento maneja esto de forma transparente: espera el retraso de backoff y lo intenta de nuevo. El host permanece ejecutándose y saludable desde la perspectiva del SO; solo el worker de suscripción está en un bucle de reintento.

### Posición de punto de control vs. posición de preparación

Las posiciones de KurrentDB tienen tanto un `CommitPosition` como un `PreparePosition`. Esta implementación almacena y restaura solo `CommitPosition` y utiliza `new Position(checkpoint.Position, checkpoint.Position)` para `FromAll.After(...)`. Para eventos escritos en una sola transacción, ambos valores son idénticos. Para transacciones de múltiples eventos pueden diferir; usar `CommitPosition` para ambos es el enfoque estándar y coincide con la propia documentación del cliente de KurrentDB.

### `StreamMessage.FellBehind`

Este mensaje es emitido por el cliente cuando la cola de procesamiento local está creciendo más rápido de lo que se consumen los eventos, lo que significa que la suscripción está bajo presión. No descarta la suscripción. Se registra en el nivel `Warning`. Si aparece con frecuencia, revise el rendimiento del manejador de eventos.

### `EventTypeMapper` y tipos de evento desconocidos

Si un tipo de evento almacenado en KurrentDB no tiene un tipo CLR correspondiente registrado en `EventTypeMapper` (p. ej., un evento de un contexto acotado diferente, un tipo renombrado o un evento heredado), `SerializerHelper.Deserialize` lanza `InvalidOperationException`. El comportamiento depende entonces de `IgnoreDeserializationErrors` (ver Manejo de errores arriba).

Para manejar tipos de evento renombrados sin ignorar errores, registre un mapa personalizado:

```csharp
EventTypeMapper.Instance.AddCustomMap<NewEventName>(
    "JordiAragonZaragoza.OldNamespace.OldEventName");

```

---

## Registro (Logging)

### Niveles relevantes

| Nivel | Evento |
| --- | --- |
| `Information` | Worker iniciado / detenido, suscripción iniciada, punto de control creado/actualizado, puesto al día hasta tiempo real |
| `Warning` | Ya suscrito (guardia), suscripción se quedó atrás, suscripción terminó inesperadamente (caída limpia), intento de reconexión |
| `Error` | Fallo de deserialización, excepción de procesamiento de eventos |
| `Debug` | Contexto de ejecución del sistema de respaldo construido, tipo de mensaje desconocido recibido, evento saltado (sin metadatos) |

### Propiedades de registro estructuradas

Todos los mensajes de registro utilizan propiedades estructuradas compatibles con Serilog, Datadog y OpenTelemetry:

| Propiedad | Descripción |
| --- | --- |
| `{SubscriptionId}` | El GUID de la suscripción |
| `{EventType}` | Cadena de tipo de evento de KurrentDB |
| `{Position}` | Posición de confirmación en el flujo `$all` |
| `{Attempt}` | Número de intento de reintento |
| `{Delay}` | Retraso de backoff en segundos |
| `{IgnoreDeserializationErrors}` | Valor de configuración en el momento del error |

### Seguimientos de OpenTelemetry

Cada evento crea una `Activity` (span) vía `EventStoreActivityRestorer.RestoreFrom(metadata, ...)`, que restaura el W3C TraceContext de los metadatos del evento. Esto vincula el span de proyección al span de comando original que produjo el evento, creando un seguimiento distribuido de extremo a extremo a través de: manejo de comando → almacén de eventos → proyección.

El span se enriquece con las siguientes etiquetas vía `EnrichActivity`:

| Etiqueta | Fuente |
| --- | --- |
| `actor.id` | `EventStoreMetadata.ActorId` |
| `actor.type` | `EventStoreMetadata.ActorType` |
| `executor` | `EventStoreMetadata.Executor` |
| `executor.type` | `EventStoreMetadata.ExecutorType` |
| `correlation.id` | `EventStoreMetadata.CorrelationId` |
| `causation.id` | `EventStoreMetadata.CausationId` (si está presente) |
| `tenant.id` | `EventStoreMetadata.TenantId` |
| `partition.id` | `EventStoreMetadata.PartitionId` (si está presente) |
| `domain.id` | `EventStoreMetadata.DomainId` (si está presente) |
| `event.occurred_at` | `EventStoreMetadata.DateOccurredOnUtc` (ISO 8601) |

Los eventos sin metadatos (semillas, migraciones, eventos nativos de KurrentDB) no producen un span vinculado. Se procesan con un `ExecutionContext` del sistema de respaldo y se registran en el nivel `Debug`.