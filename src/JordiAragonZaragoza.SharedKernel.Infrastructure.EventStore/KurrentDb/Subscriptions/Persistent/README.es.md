# Persistent Subscription to `$all`

## Overview

`KurrentDbAllStreamPersistentSubscription` implements a persistent subscription to the `$all` stream of KurrentDB using the native consumer group pattern.

**Diferencias clave frente a una catch-up subscription:**

| | CatchUp | Persistent |
|---|---|---|
| Estado del checkpoint | Cliente (archivo/BD) | Servidor (KurrentDB) |
| Distribución entre instancias | No — cada instancia lee todo | Sí — KurrentDB distribuye automáticamente |
| ACK/NACK | No aplica | Obligatorio — controla el avance del checkpoint |
| Dead-letter (parking) | No | Sí — automático tras `MaxRetryCount` NACKs |
| Caso de uso | Projector (single instance) | Reactor worker (escalable horizontalmente) |

---

## Arquitectura interna

```
AllStreamPersistentSubscriptionBackgroundWorker (BackgroundService)
│
├── ExecuteAsync(stoppingToken)
│   ├── SubscribeToAllAsync()          → conecta y retorna inmediatamente
│   └── Task.Delay(Infinite, token)    → mantiene el worker vivo
│
KurrentDbAllStreamPersistentSubscription
│
├── SubscribeToAllAsync()
│   └── ConnectToGroupAsync()
│       └── SubscribeToGroupAsync()    → intenta suscribirse
│           ├── PersistentSubscriptionNotFoundException
│           │   ├── CreateSubscriptionGroupAsync()  → crea el grupo en el servidor
│           │   └── SubscribeToAllAsync() (retry)
│           └── AlreadyExistsException              → race condition resuelta, continúa
│
├── HandleEventCallbackAsync()         → procesamiento por evento (callback del servidor)
│   ├── IsSystemEvent → ACK silencioso
│   ├── IsEventWithEmptyData → ACK silencioso
│   ├── SerializerHelper.Deserialize()
│   │   ├── Error + IgnoreDeserializationErrors=true → ACK + Warning log
│   │   └── Error + IgnoreDeserializationErrors=false → throw → NACK + retry
│   ├── unitOfWork.ExecuteInTransactionAsync()
│   │   └── eventBus.PublishAsync(domainEvent)
│   ├── ACK → avanza el checkpoint en el servidor
│   ├── OperationCanceledException (shutdown) → no ACK, no NACK, log informativo
│   └── Exception → NACK(Retry) → servidor reintenta hasta MaxRetryCount → Park
│
└── HandleSubscriptionDropped()        → callback del servidor al caer la conexión
    ├── Disposed / stoppingToken cancelled → no reconectar
    └── Cualquier otro motivo → ReconnectWithBackoffAsync() en background Task.Run
        └── SubscribeToGroupAsync() con backoff exponencial (2s → 4s → ... → 60s cap)
```

---

## Ciclo de vida del worker

El `BackgroundService` separa claramente dos responsabilidades:

1. **`SubscribeToAllAsync`** — conecta a KurrentDB y registra los callbacks. Retorna en cuanto la suscripción está establecida. No bloquea.
2. **`Task.Delay(Timeout.Infinite, stoppingToken)`** en el worker — mantiene `ExecuteAsync` vivo. Cuando el host señaliza el shutdown, el token se cancela, el `Delay` lanza `OperationCanceledException`, y `ExecuteAsync` retorna limpiamente.

Los eventos llegan por el callback `HandleEventCallbackAsync` que KurrentDB invoca en su propio canal gRPC, independientemente del `Delay`.

---

## Gestión de fallos

### Fallo en procesamiento de un evento

```
Evento falla en handler
    → NACK(Retry)
    → KurrentDB reintenta (hasta MaxRetryCount veces)
    → Si se supera MaxRetryCount → Park automático en:
       $persistentsubscription-{groupName}-parked
```

El parking es nativo del servidor. No se requiere lógica en el cliente. Los eventos aparcados se pueden inspeccionar y relanzar desde la UI de KurrentDB o via API.

### Fallo de conexión (subscription drop)

```
HandleSubscriptionDropped(reason, exception)
    │
    ├── reason == Disposed → host parando, no reconectar
    ├── stoppingToken.IsCancellationRequested → no reconectar
    └── cualquier otro motivo:
        Task.Run → ReconnectWithBackoffAsync()
            ├── ServerError: delay inicial 2s
            └── Otros: delay inicial 10s
            Backoff: delay * 2 por intento fallido, cap 60s
            Loop: while (isRunning && isDropped && !cancelled)
```

La reconexión reutiliza `SubscribeToGroupAsync`, que incluye la creación automática del grupo si fuera necesario.

### Shutdown del host durante procesamiento

Si el `stoppingToken` se cancela mientras se procesa un evento, el `OperationCanceledException` se captura explícitamente **sin hacer NACK**. El servidor detecta la desconexión y reentrega el evento al siguiente consumer disponible del grupo.

---

## Configuración

### `KurrentDbAllStreamPersistentSubscriptionOptions`

| Propiedad | Tipo | Default | Descripción |
|---|---|---|---|
| `GroupName` | `string` | `"all-stream-group"` | Nombre del consumer group en KurrentDB. Todas las instancias del mismo worker deben usar el mismo nombre. |
| `BufferSize` | `int` | `10` | Máximo de eventos en vuelo simultáneos que el cliente acepta del servidor. |
| `MaxRetryCount` | `int` | `5` | Número de reintentos del servidor antes de aparcar el evento en el stream de dead-letter. Se configura en el servidor al crear el grupo, no en el cliente. |
| `IgnoreDeserializationErrors` | `bool` | `true` | Si `true`, los eventos que fallen la deserialización se ACK'an y se omiten con un warning. Si `false`, se NACK'an y entran en el ciclo de retry/park. Útil en `false` para detectar regresiones de esquema en producción estable. |
| `ResolveLinkTos` | `bool` | `false` | Si `true`, resuelve los link events al evento original. |
| `FilterOptions` | `SubscriptionFilterOptions?` | `null` | Filtro server-side aplicado al crear el grupo. No afecta a grupos ya existentes. |
| `Credentials` | `UserCredentials?` | `null` | Credenciales para el cliente KurrentDB. Si `null` usa las del cliente registrado en DI. |

> **Importante:** `MaxRetryCount` y `FilterOptions` solo tienen efecto en la **creación** del grupo. Si el grupo ya existe en KurrentDB, modificar estas opciones no lo actualiza. Para cambiarlas en un grupo existente hay que eliminarlo y recrearlo.

---

## Registro en DI

### Configuración por defecto

```csharp
// Program.cs — Worker Reactor
builder.Services
    .AddSharedKernelInfrastructureKurrentDbAllStreamPersistentSubscription();
```

### Configuración personalizada

Crea una extensión en tu proyecto si necesitas opciones distintas a las por defecto:

```csharp
public static class ReactorSubscriptionExtensions
{
    public static IServiceCollection AddCinemaReactorPersistentSubscription(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new KurrentDbAllStreamPersistentSubscriptionOptions();
        configuration.GetSection("PersistentSubscription").Bind(options);

        services.AddTransient<KurrentDbAllStreamPersistentSubscription>();

        return services.AddHostedService(serviceProvider =>
        {
            var subscription = serviceProvider
                .GetRequiredService<KurrentDbAllStreamPersistentSubscription>();
            var logger = serviceProvider
                .GetRequiredService<ILogger<AllStreamPersistentSubscriptionBackgroundWorker>>();

            return new AllStreamPersistentSubscriptionBackgroundWorker(
                subscription,
                options,
                logger);
        });
    }
}
```

```json
// appsettings.json
{
  "PersistentSubscription": {
    "GroupName": "cinema-reservation-reactor",
    "BufferSize": 20,
    "MaxRetryCount": 10,
    "IgnoreDeserializationErrors": false,
    "ResolveLinkTos": false
  }
}
```

---

## Ejemplo completo — Worker Reactor

```csharp
// Worker.Reactor/Program.cs
public static async Task Main(string[] args)
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    var configuration = builder.Configuration;

    builder.AddInfrastructure();
    builder.AddInfrastructureKurrentDbClient();

    // Registros específicos del Reactor
    builder.Services
        .AddDomain()
        .AddApplication()
        .AddApplicationValidators()
        .AddApplicationCommandHandlers()
        .AddApplicationPolicies(configuration)       // registra las políticas/reactores
        .AddInfrastructureEventStoreRepositories();

    // Registros del SharedKernel
    builder.Services
        .AddSharedKernelDomain()
        .AddSharedKernelApplicationCommandBus()
        .AddSharedKernelInfrastructureKurrentDbBusiness()
        .AddSharedKernelInfrastructure()             // incluye IExecutionContextService (AsyncLocal)
        .AddSharedKernelInfrastructureReactorBus()   // IEventBus + ICommandBus
        .AddSharedKernelInfrastructureKurrentDbAllStreamPersistentSubscription();

    IHost app = builder.Build();
    await app.RunAsync();
}
```

---

## Ejemplo de política (reactor)

Las políticas reciben eventos a través del bus en memoria. El flujo completo es:

```
KurrentDB $all
    → HandleEventCallbackAsync (deserializa + establece ExecutionContext)
    → unitOfWork.ExecuteInTransactionAsync
        → eventBus.PublishAsync(domainEvent)
            → MediatR → ScheduleShowtimeInAuditoriumPolicy.HandleAsync
                → commandBus.SendAsync(new ScheduleShowtimeInAuditoriumCommand(...))
                    → MediatR → command handler → aggregate → evento guardado en KurrentDB
    → ACK al servidor
```

```csharp
public sealed class ScheduleShowtimeInAuditoriumPolicy
    : BaseEventHandler<ShowtimeScheduledEvent>
{
    private readonly ICommandBus commandBus;

    public ScheduleShowtimeInAuditoriumPolicy(ICommandBus commandBus)
        => this.commandBus = commandBus
            ?? throw new ArgumentNullException(nameof(commandBus));

    public override async Task HandleAsync(
        ShowtimeScheduledEvent @event,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await this.commandBus.SendAsync(
            new ScheduleShowtimeInAuditoriumCommand(
                @event.AuditoriumId,
                @event.AggregateId),
            cancellationToken);
    }
}
```

---

## Escalado horizontal

Todas las instancias del worker deben usar el mismo `GroupName`. KurrentDB distribuye los eventos del grupo entre los consumers conectados automáticamente (round-robin por defecto, configurable con `ConsumerStrategy`).

```yaml
# kubernetes/reactor-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cinema-reactor-worker
spec:
  replicas: 3          # KurrentDB distribuye los eventos entre las 3 instancias
  template:
    spec:
      containers:
      - name: reactor-worker
        image: cinema-reservation-worker-reactor:latest
        env:
        - name: PersistentSubscription__GroupName
          value: "cinema-reservation-reactor"
        - name: ConnectionStrings__KurrentDb
          value: "esdb://kurrentdb:2113?tls=false"
```

```bash
# Escalar en caliente — KurrentDB redistribuye automáticamente
kubectl scale deployment cinema-reactor-worker --replicas=5
```

> **Race condition en el arranque:** si varias instancias arrancan simultáneamente y el grupo no existe, pueden intentar crearlo al mismo tiempo. La implementación lo gestiona: la primera que crea el grupo tiene éxito; las demás reciben un `RpcException(AlreadyExists)` que se captura silenciosamente, y todas continúan suscribiéndose al grupo ya existente.

---

## Corner cases

### El grupo ya existe con configuración diferente

Si cambias `MaxRetryCount` o `FilterOptions` en las opciones pero el grupo ya existe en KurrentDB, los nuevos valores **no se aplican**. El servidor mantiene la configuración original del grupo.

**Solución:** eliminar el grupo desde la UI de KurrentDB o via API y reiniciar el worker. El grupo se recreará con la nueva configuración.

### Evento permanentemente inprocesable

Si un handler tiene un bug que hace fallar siempre el mismo evento:

1. El servidor reintenta `MaxRetryCount` veces.
2. Al superar el límite, el evento se aparca en `$persistentsubscription-{groupName}-parked`.
3. El worker continúa procesando el siguiente evento — no se bloquea.
4. Corrige el bug, redespliega, y relanza el evento aparcado desde la UI.

### Desconexión durante el procesamiento

Si el worker pierde la conexión mientras procesa un evento (antes del ACK):

- KurrentDB detecta la desconexión.
- El evento se reencola para otro consumer del grupo (o para el mismo cuando reconecte).
- El evento puede procesarse dos veces si el handler ya completó su lógica pero no llegó a hacer ACK. Los handlers deben ser **idempotentes**.

### Shutdown durante el procesamiento

Si el host para (`Ctrl+C`, SIGTERM) mientras un evento está siendo procesado:

- `stoppingToken` se cancela.
- `OperationCanceledException` se captura sin hacer NACK.
- El servidor reentrega el evento al próximo consumer disponible.
- No hay pérdida de eventos, pero puede haber doble procesamiento si el handler ya completó su trabajo. Idempotencia obligatoria.

---

## Logging

### Niveles relevantes

| Nivel | Qué loguea |
|---|---|
| `Information` | Arranque, conexión establecida, reconexiones, fallback de execution context |
| `Warning` | Subscription drop, errores de deserialización ignorados |
| `Error` | Fallos de procesamiento de eventos, fallos de NACK, errores de reconexión |
| `Debug` | Eventos de sistema skipeados, eventos procesados y ACK'd, metadata ausente |

```json
{
  "Logging": {
    "LogLevel": {
      "JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent": "Debug"
    }
  }
}
```

### Trazas en Datadog / OpenTelemetry

Cada evento procesado restaura el `Activity` desde la metadata del evento (`traceparent`/`tracestate` via `EventStoreActivityRestorer`), propagando la traza distribuida original. Los tags añadidos al span incluyen `actor.id`, `correlation.id`, `causation.id`, `tenant.id`, y `event.occurred_at`.

---

## Monitorización desde la UI de KurrentDB

**Ruta:** `Persistent Subscriptions` → seleccionar el grupo

| Métrica | Descripción |
|---|---|
| **Connected** | Número de instancias del worker actualmente conectadas al grupo |
| **Last checkpoint** | Posición del último evento ACK'd — indica el progreso |
| **In-flight** | Eventos entregados pero sin ACK todavía (≤ `BufferSize`) |
| **Parked messages** | Eventos que superaron `MaxRetryCount` — requieren atención manual |

**Regla práctica:** si _Parked messages_ crece, revisar los logs de error del worker. Si _Connected_ es 0 y el worker debería estar activo, hay un problema de conectividad o el host no arrancó correctamente.