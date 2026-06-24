# 🧠 ExecutionContext

## 📌 Propósito

El `ExecutionContext` (Contexto de Ejecución) es el mecanismo central para:

* Propagar **información de identidad y trazabilidad**.
* Mantener un **contexto consistente** entre capas.
* Habilitar una **auditoría completa**.
* Facilitar la **observabilidad (registros, rastreo, depuración)**.

Se establece **una vez por solicitud/mensaje** y se accede a él globalmente
durante toda su ejecución.

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
|---|---|---|
| Usuario autenticado | `user:123e4567-e89b-12d3-a456-426614174000` | user |
| Worker reaccionando a un evento de usuario | `user:123...` (propagado) | user |
| Consumer procesando evento de integración | `service:cinema-reactor` | system |
| Trabajo por lotes | `job:daily-cleanup` | system |
| Registro anónimo | `external:192.168.1.1` | external |
| Webhook entrante | `external:10.0.0.5` | external |

> **Importante:** El Actor **no cambia a lo largo del flujo**. Un worker que
> reacciona a una acción de un usuario propaga el usuario original como actor.
> Un consumer que procesa un evento de integración sin actor de usuario conocido
> usa `ActorType.System`.

#### Implicaciones de `ActorType` en autorización

| ActorType | `ValidateScopeAsync` | `AuthorizeAsync` |
|---|---|---|
| `User` | ✅ ejecutado | ✅ completo (roles + permisos + políticas) |
| `System` | ❌ no aplica | `Success` sin evaluación (trusted) |
| `External` | ❌ omitido | `Forbidden` — error de configuración si llega aquí |

> `ActorType.External` solo debe aparecer en endpoints `[AllowAnonymous]` que
> no declaran `[Authorize]`. Si un actor External llega a `AuthorizeAsync`, el
> sistema devuelve `Forbidden` — es un error de configuración del endpoint.

#### Formato de ActorId

El `ActorId` debe comenzar con uno de los siguientes prefijos conocidos seguido de un identificador no vacío:

```
user:{guid}        — usuario autenticado de la plataforma
service:{name}     — servicio interno actuando de forma autónoma
job:{name}         — trabajo programado o por lotes
external:{name}    — llamante externo no autenticado (IP, fuente de webhook, etc.)
```

Métodos de fábrica: `CreateUserActorId`, `CreateServiceActorId`,
`CreateJobActorId`, `CreateExternalActorId`.

---

### 2. Executor (quien ejecuta técnicamente)

Representa el sistema que está ejecutando realmente la acción.

```csharp
string Executor           // ej. "cinema-reservation-api-command"
ExecutorType ExecutorType // SmartEnum: service | worker | tool
```

| Servicio | Executor | ExecutorType |
|---|---|---|
| API | `cinema-reservation-api-command` | service |
| Worker | `cinema-reservation-reactor-worker` | worker |

> Permite responder a: "¿Quién está ejecutando esto realmente?" y "¿Qué servicio está fallando?".

---

### 3. Correlación y Causalidad

```csharp
Guid  CorrelationId  // Requerido. Nunca vacío.
Guid? CausationId    // Opcional. Identifica el evento desencadenante.
```

**CorrelationId** — identifica toda la cadena de ejecución. Se genera en el
punto de entrada y se propaga siempre.

**CausationId** — identifica el evento o command específico que desencadenó
directamente esta request. Permite reconstruir el grafo completo de causa y efecto.

```
Solicitud HTTP → CorrelationId=A, CausationId=null
  → UserRegisteredEvent (Id=E1, CorrelationId=A)
  → POST /send-welcome-email
      x-correlation-id: A
      x-causation-id:   E1
      → CorrelationId=A, CausationId=E1
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
* Partition → subdivisión regional o área.
* Domain → instancia de contexto delimitado (ej. un cine).

Para actores External sin `x-tenant-id`, se usa `SystemConstants.SystemTenantId`
como fallback. Cada endpoint es responsable de aplicar requisitos más estrictos.

---

## ⚙️ Cómo funciona

### 📍 1. En la entrada HTTP (ExecutionContextMiddleware)

1. **Omisión de infraestructura** — rutas con prefijos conocidos (`/swagger`,
   `/health`, etc.) omiten toda la lógica de contexto.
2. **Resolver actor**:
   - JWT con `oid` válido → `ActorType.User`, `actorId = user:{guid}`
   - JWT sin `oid` válido → **401**
   - Sin JWT + `[AllowAnonymous]` → `ActorType.External`, `actorId = external:{ip}`
   - Sin JWT sin `[AllowAnonymous]` → **401**
3. **Resolver tenant**:
   - `User` sin `x-tenant-id` → **400**
   - `External` sin `x-tenant-id` → `SystemTenantId` como fallback
4. **Resolver** `CorrelationId` (genera uno nuevo si ausente) y `CausationId`
   (opcional).
5. **Resolver** `x-partition-id` y `x-domain-id` opcionales.
6. **Abrir ámbito de logging** estructurado con todos los valores.
7. **Construir** el `ExecutionContext`.
8. **`ValidateScopeAsync`** — solo para `ActorType.User`. → **403** si falla.
9. **`SetExecutionContext`**.
10. **Añadir** `x-correlation-id` a la respuesta.
11. **Limpiar** en el bloque `finally`.

### 📍 2. En workers/consumers (sin HTTP)

El `ExecutionContext` se construye directamente desde los metadatos del mensaje
o evento, sin pasar por el middleware HTTP:

- **KurrentDB subscription** (`KurrentDbAllStreamSubscription`): reconstruye
  el contexto desde `EventStoreMetadata` vía `EventStoreMetadata.ToExecutionContext()`.
  Si no hay metadata válida, usa un contexto de sistema de fallback.
- **Consumers de eventos de integración**: reconstruyen el contexto desde los
  metadatos del mensaje propagados (actor, correlationId, causationId, tenant).

En ambos casos se usa `OverrideExecutionContext` en lugar de `SetExecutionContext`
porque el contexto se establece en el ámbito del consumer, no desde el middleware.

---

### 📍 3. Almacenamiento

```csharp
AsyncLocal<ExecutionContext?>
```

- **Aislamiento de solicitudes** — sin estado compartido entre requests paralelas.
- **Propagación async/await** — el contexto fluye naturalmente.
- `SetExecutionContext` lanza `InvalidOperationException` si ya hay contexto —
  previene sobrescrituras accidentales.
- `OverrideExecutionContext` — solo para infraestructura que reconstruye el
  contexto desde metadatos externos. **Nunca desde código de aplicación o dominio.**

---

## 🧾 Registro (logging) integrado

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

### HTTP saliente (DelegatingHandler)

```
x-correlation-id, x-causation-id, x-tenant-id, x-partition-id, x-domain-id
```

### Mensajería (MassTransit / RabbitMQ)

```json
{
  "correlationId": "...", "causationId": "...",
  "actorId": "...", "actorType": "...",
  "tenantId": "...", "partitionId": "...", "domainId": "..."
}
```

### EventStoreMetadata (KurrentDB)

```json
{
  "actorId": "user:123...", "actorType": "user",
  "executor": "cinema-reservation-api-command", "executorType": "service",
  "correlationId": "aaa...", "causationId": null,
  "tenantId": "bbb...",
  "traceParent": "00-{traceId}-{spanId}-01",
  "dateOccurredOnUtc": "2025-05-30T14:32:00Z"
}
```

El `ExecutionContext` se reconstruye desde estos metadatos por
`KurrentDbAllStreamSubscription` para cada evento consumido.

---

## 🚫 Decisiones de diseño

### ❌ No permitir acceso no autenticado sin consentimiento explícito

Endpoints que aceptan `External` deben marcarse `[AllowAnonymous]`. Sin JWT +
sin `[AllowAnonymous]` → 401.

### ❌ `ActorType.External` nunca puede invocar commands con `[Authorize]`

`AuthorizeAsync` devuelve `Forbidden` para External — es un error de
configuración si llega ahí, no un caso de uso legítimo.

### ❌ No usar Actor como Executor

Evita errores de auditoría donde un fallo de worker parece culpa del usuario.

### ❌ No validar dentro de ExecutionContextService

| Componente | Responsabilidad |
|---|---|
| Middleware / Worker | Construcción + validación |
| ExecutionContextService | Solo almacenamiento |

### ❌ No usar enumeraciones simples

`SmartEnum` elimina magic strings, es extensible y serializa limpiamente.

---

## ✅ Convenciones

### Encabezados HTTP

| Encabezado | User | External |
|---|---|---|
| `x-tenant-id` | ✅ requerido | opcional (fallback SystemTenantId) |
| `x-correlation-id` | opcional | opcional |
| `x-causation-id` | opcional | opcional |
| `x-partition-id` | opcional | opcional |
| `x-domain-id` | opcional | opcional |

### Reglas de validación del ExecutionContext

| Campo | Regla |
|---|---|
| `ActorId` | No vacío, debe coincidir con prefijo conocido |
| `CorrelationId` | `Guid` no vacío |
| `TenantId` | `Guid` no vacío (SystemTenantId para External) |
| `PartitionId` | `null` o `Guid` no vacío |
| `DomainId` | `null` o `Guid` no vacío |

---

## 🧪 Ejemplos completos

### Usuario autenticado

```json
{
  "ActorId": "user:123...", "ActorType": "user",
  "Executor": "cinema-reservation-api-command", "ExecutorType": "service",
  "CorrelationId": "2222...", "CausationId": null,
  "ScopeContext": { "TenantId": "1111...", "PartitionId": null, "DomainId": null }
}
```

### Actor externo (registro anónimo)

```json
{
  "ActorId": "external:192.168.1.1", "ActorType": "external",
  "Executor": "cinema-reservation-api-command", "ExecutorType": "service",
  "CorrelationId": "3333...", "CausationId": null,
  "ScopeContext": { "TenantId": "00000000-0000-0000-0000-000000000001", "PartitionId": null, "DomainId": null }
}
```

### Consumer de evento de integración (ActorType.System)

```json
{
  "ActorId": "service:cinema-reservation-reactor", "ActorType": "system",
  "Executor": "cinema-reservation-reactor-worker", "ExecutorType": "worker",
  "CorrelationId": "aaa...", "CausationId": "E1...",
  "ScopeContext": { "TenantId": "1111...", "PartitionId": null, "DomainId": null }
}
```

---

## 🧠 Modelo mental

| Concepto | Pregunta que responde |
|---|---|
| **Actor** | ¿Quién quería que esto sucediera? |
| **Executor** | ¿Quién lo está ejecutando realmente? |
| **CorrelationId** | ¿Cuál es la cadena completa de eventos? |
| **CausationId** | ¿Qué evento específico desencadenó este paso? |
| **Scope** | ¿En qué contexto de negocio se ejecuta? |

---

## 🚀 Beneficios

* Auditoría real y confiable a través de flujos síncronos y asíncronos.
* Depuración distribuida mediante correlación y causalidad.
* Observabilidad completa: logs estructurados + trazas OTel end-to-end.
* Desacoplamiento entre servicios mediante contratos de headers consistentes.
* Preparado para event sourcing: el contexto viaja en `EventStoreMetadata`.
* Integrado con el sistema de autorización: `ActorType` determina el nivel de
  verificación aplicado en cada request.