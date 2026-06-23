# 🧠 ExecutionContext

## 📌 Propósito

El `ExecutionContext` (Contexto de Ejecución) es el mecanismo central para:

* Propagar **información de identidad y trazabilidad**.
* Mantener un **contexto consistente** entre capas.
* Habilitar una **auditoría completa**.
* Facilitar la **observabilidad (registros, rastreo, depuración)**.

Se establece **una vez por solicitud/mensaje** y se accede a él globalmente durante toda su ejecución.

---

## 🧩 Conceptos clave

### 1. Actor (quien origina la intención)

Representa **quién inició la acción**, no quién la ejecuta técnicamente.

```csharp
string ActorId      // Formato: "{prefijo}:{identificador}"
ActorType ActorType // SmartEnum: user | system | external

```

#### Ejemplos

| Caso | ActorId | ActorType |
| --- | --- | --- |
| Usuario autenticado | `user:123e4567-e89b-12d3-a456-426614174000` | user |
| Worker reaccionando a un evento | `user:123...` (propagado desde el origen) | user |
| Trabajo por lotes (Batch) | `job:daily-cleanup` | system |
| Registro anónimo | `external:192.168.1.1` | external |
| Webhook entrante | `external:10.0.0.5` | external |

> **Importante:** El Actor **no cambia a lo largo del flujo**. Un worker que reacciona a una acción de un usuario propaga al usuario original como el actor.

#### Formato de ActorId

El `ActorId` debe comenzar con uno de los siguientes prefijos conocidos seguido de un identificador no vacío:

```
user:{guid}        — usuario autenticado de la plataforma
service:{name}     — servicio interno actuando de forma autónoma
job:{name}         — trabajo programado o por lotes
external:{name}    — llamante externo no autenticado (IP, fuente de webhook, etc.)

```

Se aplica en tiempo de construcción mediante `ExecutionContext.IsValidActorIdFormat(actorId)`.
Métodos de fábrica: `CreateUserActorId`, `CreateServiceActorId`, `CreateJobActorId`, `CreateExternalActorId`.

---

### 2. Executor (quien ejecuta técnicamente)

Representa el sistema que está ejecutando realmente la acción.

```csharp
string Executor           // ej. "cinema-reservation-api-command"
ExecutorType ExecutorType // SmartEnum: service | worker | tool

```

#### Ejemplos

| Servicio | Executor | ExecutorType |
| --- | --- | --- |
| API | `cinema-reservation-api-command` | service |
| Worker | `cinema-reservation-reactor-worker` | worker |

> Permite responder a: "¿Quién está ejecutando esto realmente?" y "¿Qué servicio está fallando?".

---

### 3. Correlación y Causalidad

```csharp
Guid  CorrelationId  // Requerido. Nunca vacío.
Guid? CausationId    // Opcional. Identifica el evento desencadenante.

```

#### CorrelationId

* Identifica **toda la cadena de ejecución** desde el punto de entrada hasta la finalización.
* Generado en el punto de entrada (API Gateway o primer servicio).
* Siempre se propaga mediante el encabezado `x-correlation-id`.
* Se genera un nuevo `Guid` si el encabezado está ausente o vacío.

#### CausationId

* Identifica **el evento o comando específico que desencadenó directamente esta solicitud** — distinto del `CorrelationId` que rastrea toda la cadena.
* Extraído del encabezado `x-causation-id`.
* `null` para solicitudes HTTP directas no provocadas por un evento previo.
* Uso típico: un manejador de reacción reenvía el ID del evento al que está reaccionando, de modo que se pueda reconstruir el gráfico completo de causa y efecto.

#### Ejemplo de flujo

```
Solicitud HTTP (acción de usuario)
  CorrelationId = A
  CausationId   = null

  → UserRegisteredEvent publicado
      Id            = E1
      CorrelationId = A
      CausationId   = null

  → Reacción: POST /send-welcome-email
      x-correlation-id: A       (misma cadena)
      x-causation-id:   E1      (el evento que causó esta llamada)

      CorrelationId = A
      CausationId   = E1

```

---

### 4. ScopeContext (ámbito de negocio)

```csharp
Guid  TenantId     // Requerido. Nunca vacío.
Guid? PartitionId  // Opcional. No debe ser Guid.Empty si se proporciona.
Guid? DomainId     // Opcional. No debe ser Guid.Empty si se proporciona.

```

Representa el **ámbito funcional** donde ocurre la acción:

* Tenant → empresa u organización.
* Partition → subdivisión regional o área dentro de un tenant.
* Domain → instancia de contexto delimitado específico (ej. un cine).

Para actores no autenticados (External) donde no se proporciona un encabezado de tenant, se utiliza un ID de tenant a nivel de sistema como respaldo. Cada endpoint es responsable de aplicar requisitos de tenant más estrictos si es necesario.

---

## ⚙️ Cómo funciona

### 📍 1. En la entrada HTTP

El `ExecutionContextMiddleware` se ejecuta una vez por solicitud en este orden:

1. **Omisión de infraestructura** — las rutas que coinciden con prefijos de infraestructura conocidos (`/swagger`, `/health`, `/metrics`, etc.) omiten toda la lógica de contexto por completo.
2. **Resolver actor** — intenta extraer el claim `oid` del JWT:
* JWT presente y `oid` válido → `ActorType.User`, `actorId = user:{guid}`
* JWT presente pero sin `oid` válido → **401 No autorizado**
* Sin JWT + endpoint tiene `[AllowAnonymous]` → `ActorType.External`, `actorId = external:{clientIp}`
* Sin JWT + endpoint requiere autenticación → **401 No autorizado**


3. **Resolver tenant** desde el encabezado `x-tenant-id`:
* `ActorType.User` sin un tenant válido → **400 Solicitud incorrecta**
* `ActorType.External` sin un tenant → utiliza `SystemConstants.SystemTenantId` como respaldo; el endpoint puede aplicar requisitos más estrictos.


4. **Resolver** `CorrelationId` desde `x-correlation-id` (genera un nuevo `Guid` si está ausente o vacío) y `CausationId` desde `x-causation-id` (opcional; identifica el evento previo que activó esta solicitud).
5. **Resolver encabezados de ámbito opcionales** (`x-partition-id`, `x-domain-id`), tratando los valores ausentes o `Guid.Empty` como `null`.
6. **Abrir ámbito de registro estructurado** con todos los valores resueltos para que cada línea de registro en la solicitud lleve el contexto completo automáticamente.
7. **Construir** el `ExecutionContext`.
8. **Validar autorización de ámbito** mediante `IAuthorizationService.ValidateScopeAsync` — solo para `ActorType.User`. Los actores externos omiten este paso ya que aún no tienen identidad en la plataforma. → **403 Prohibido** en caso de fallo.
9. **Establecer** el contexto mediante `IExecutionContextService.SetExecutionContext`.
10. **Añadir** `x-correlation-id` a los encabezados de respuesta.
11. **Limpiar** el contexto en el bloque `finally` después de que la tubería se complete.

---

### 📍 2. Almacenamiento

```csharp
AsyncLocal<ExecutionContext?>

```

Garantías:

* **Aislamiento de solicitudes** — cada solicitud tiene su propio contexto independiente.
* **Seguridad en ejecución concurrente** — no hay estado compartido entre solicitudes paralelas.
* **Propagación async/await** — el contexto fluye naturalmente a través de los límites de `await`.

`SetExecutionContext` lanza una `InvalidOperationException` si se llama más de una vez dentro del mismo contexto asíncrono, evitando sobrescrituras accidentales.

`OverrideExecutionContext` omite esa protección y está reservado exclusivamente para código de infraestructura que reconstruye el contexto a partir de metadatos externos (ej. manejadores de suscripción de KurrentDB). Nunca debe ser llamado desde código de aplicación o de dominio.

---

## 🧾 Registro (logging) integrado

Se abre un ámbito estructurado al inicio de cada solicitud:

```csharp
logger.BeginScope(new Dictionary<string, object?>
{
    ["CorrelationId"] = correlationId,
    ["ActorId"]       = actorId,
    ["ActorType"]     = actorType.Name,
    ["Executor"]      = executor,
    ["ExecutorType"]  = executorType.Name,
    ["TenantId"]      = effectiveTenantId,
    ["PartitionId"]   = partitionId,   // omitido por Serilog cuando es null
    ["DomainId"]      = domainId,      // omitido por Serilog cuando es null
})

```

---

## 🔁 Propagación de contexto

### HTTP saliente

Propagar mediante un `DelegatingHandler`:

```
x-correlation-id
x-causation-id
x-tenant-id
x-partition-id
x-domain-id

```

### Mensajería (MassTransit / RabbitMQ)

Incluir en los metadatos del mensaje:

```json
{
  "correlationId": "...",
  "causationId":   "...",
  "actorId":       "...",
  "actorType":     "...",
  "tenantId":      "...",
  "partitionId":   "...",
  "domainId":      "..."
}

```

### Metadatos de eventos de KurrentDB

Almacenado en `EventStoreMetadata` junto al TraceContext de W3C:

```json
{
  "actorId":          "user:123...",
  "actorType":        "user",
  "executor":         "cinema-reservation-api-command",
  "executorType":     "service",
  "correlationId":    "aaa...",
  "causationId":      null,
  "tenantId":         "bbb...",
  "traceParent":      "00-{traceId}-{spanId}-01",
  "dateOccurredOnUtc":"2025-05-30T14:32:00Z"
}

```

El `ExecutionContext` es reconstruido a partir de estos metadatos por el manejador de suscripción de KurrentDB para cada evento consumido.

---

## 🚫 Decisiones de diseño

### ❌ No permitir acceso no autenticado sin consentimiento explícito

Los endpoints que aceptan actores `External` deben estar marcados con `[AllowAnonymous]`. El middleware devuelve 401 para cualquier solicitud no autenticada a un endpoint que no lleve ese atributo, incluso si el endpoint no está protegido de otra manera por una política de autorización.

### ❌ No usar Actor como Executor

Evita errores de auditoría donde un error de un worker parece culpa del usuario.

### ❌ No validar dentro de ExecutionContextService

| Componente | Responsabilidad |
| --- | --- |
| Middleware | Construcción + validación |
| Servicio | Solo almacenamiento |

### ❌ No usar enumeraciones simples (plain enums)

`SmartEnum` elimina las cadenas mágicas (magic strings), es extensible sin cambios que rompan la compatibilidad y se serializa limpiamente.

---

## ✅ Convenciones

### Encabezados

| Encabezado | Requerido para User | Requerido para External |
| --- | --- | --- |
| `x-tenant-id` | ✅ | opcional (respaldo del sistema) |
| `x-correlation-id` | opcional | opcional |
| `x-causation-id` | opcional | opcional |
| `x-partition-id` | opcional | opcional |
| `x-domain-id` | opcional | opcional |

### Reglas de validación

| Campo | Regla |
| --- | --- |
| `ActorId` | No vacío, debe coincidir con un patrón de prefijo conocido |
| `CorrelationId` | `Guid` no vacío |
| `TenantId` | `Guid` no vacío (GUID del sistema para External) |
| `PartitionId` | `null` o `Guid` no vacío |
| `DomainId` | `null` o `Guid` no vacío |

---

## 🧪 Ejemplos completos

### Solicitud de usuario autenticado

```
POST /showtimes
Headers:
  Authorization:    Bearer {jwt con claim oid}
  x-tenant-id:      1111...
  x-correlation-id: 2222...

```

```json
{
  "ActorId":       "user:123...",
  "ActorType":     "user",
  "Executor":      "cinema-reservation-api-command",
  "ExecutorType":  "service",
  "CorrelationId": "2222...",
  "CausationId":   null,
  "ScopeContext": {
    "TenantId":    "1111...",
    "PartitionId": null,
    "DomainId":    null
  }
}

```

### Solicitud externa no autenticada (ej. registro)

```
POST /auth/register   [AllowAnonymous]
Headers:
  x-correlation-id: 3333...
  (sin encabezado Authorization, sin x-tenant-id)

```

```json
{
  "ActorId":       "external:192.168.1.1",
  "ActorType":     "external",
  "Executor":      "cinema-reservation-api-command",
  "ExecutorType":  "service",
  "CorrelationId": "3333...",
  "CausationId":   null,
  "ScopeContext": {
    "TenantId":    "00000000-0000-0000-0000-000000000001",
    "PartitionId": null,
    "DomainId":    null
  }
}

```

---

## 🧠 Modelo mental

| Concepto | Pregunta que responde |
| --- | --- |
| **Actor** | ¿Quién quería que esto sucediera? |
| **Executor** | ¿Quién lo está ejecutando realmente? |
| **CorrelationId** | ¿Cuál es la cadena completa de eventos? |
| **CausationId** | ¿Qué evento específico desencadenó este paso? |
| **Scope** | ¿En qué contexto de negocio se ejecuta? |

---

## 🚀 Beneficios

* Auditoría real y confiable a través de flujos síncronos y asíncronos.
* Depuración distribuida sencilla mediante correlación y causalidad.
* Observabilidad completa: registros estructurados + trazas de OTel vinculadas de principio a fin.
* Desacoplamiento entre servicios mediante contratos de encabezados consistentes.
* Preparado para *event sourcing*: el contexto viaja de ida y vuelta a través de los metadatos de KurrentDB.