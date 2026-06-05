# 🧠 ExecutionContext

## 📌 Purpose

The `ExecutionContext` is the central mechanism for:

* Propagating **identity and traceability information**
* Maintaining **consistent context** across layers
* Enabling **complete auditability**
* Facilitating **observability (logs, tracing, debugging)**

It is established **once per request / message** and accessed globally
throughout its execution.

---

## 🧩 Key concepts

### 1. Actor (who originates the intent)

Represents **who initiated the action**, not who executes it technically.

```csharp
string ActorId      // Format: "{prefix}:{identifier}"
ActorType ActorType // SmartEnum: user | system | external
```

#### Examples

| Case                        | ActorId                                      | ActorType |
|-----------------------------|----------------------------------------------|-----------|
| Authenticated user          | `user:123e4567-e89b-12d3-a456-426614174000`  | user      |
| Worker reacting to an event | `user:123...` (propagated from origin)       | user      |
| Batch job                   | `job:daily-cleanup`                          | system    |
| Anonymous registration      | `external:192.168.1.1`                       | external  |
| Incoming webhook            | `external:10.0.0.5`                          | external  |

> **Important:** The Actor **does not change throughout the flow**. A worker
> reacting to a user action propagates the original user as the actor.

#### ActorId format

The `ActorId` must start with one of the following known prefixes followed
by a non-empty identifier:

```
user:{guid}        — authenticated platform user
service:{name}     — internal service acting autonomously
job:{name}         — scheduled or batch job
external:{name}    — unauthenticated external caller (IP, webhook source, etc.)
```

Enforced at construction time via `ExecutionContext.IsValidActorIdFormat(actorId)`.
Factory methods: `CreateUserActorId`, `CreateServiceActorId`,
`CreateJobActorId`, `CreateExternalActorId`.

---

### 2. Executor (who executes technically)

Represents the system that is actually running the action.

```csharp
string Executor           // e.g. "cinema-reservation-api-command"
ExecutorType ExecutorType // SmartEnum: service | worker | tool
```

#### Examples

| Service | Executor                            | ExecutorType |
|---------|-------------------------------------|--------------|
| API     | `cinema-reservation-api-command`    | service      |
| Worker  | `cinema-reservation-reactor-worker` | worker       |

> Allows answering: "Who is actually running this?" and "Which service
> is failing?"

---

### 3. Correlation & Causation

```csharp
Guid  CorrelationId  // Required. Never empty.
Guid? CausationId    // Optional. Identifies the triggering event.
```

#### CorrelationId

* Identifies **the entire execution chain** from entry point to completion
* Generated at the entry point (API Gateway or first service)
* Always propagated via `x-correlation-id` header
* A new `Guid` is generated if the header is absent or empty

#### CausationId

* Identifies **the specific event or command that directly triggered
  this request** — distinct from `CorrelationId` which tracks the whole chain
* Extracted from the `x-causation-id` header
* `null` for direct HTTP requests not triggered by a prior event
* Typical usage: a reaction handler forwards the ID of the event it is
  reacting to, so the full cause-and-effect graph can be reconstructed

#### Flow example

```
HTTP Request (user action)
  CorrelationId = A
  CausationId   = null

  → UserRegisteredEvent published
      Id            = E1
      CorrelationId = A
      CausationId   = null

  → Reaction: POST /send-welcome-email
      x-correlation-id: A       (same chain)
      x-causation-id:   E1      (the event that caused this call)

      CorrelationId = A
      CausationId   = E1
```

---

### 4. ScopeContext (business scope)

```csharp
Guid  TenantId     // Required. Never empty.
Guid? PartitionId  // Optional. Must not be Guid.Empty if provided.
Guid? DomainId     // Optional. Must not be Guid.Empty if provided.
```

Represents the **functional scope** where the action occurs:

* Tenant → company or organization
* Partition → regional subdivision or area within a tenant
* Domain → specific bounded context instance (e.g. a cinema)

For unauthenticated (External) actors where no tenant header is provided,
a system-level tenant ID is used as fallback. Each endpoint is responsible
for enforcing stricter tenant requirements if needed.

---

## ⚙️ How it works

### 📍 1. On HTTP entry

The `ExecutionContextMiddleware` runs once per request in this order:

1. **Infrastructure bypass** — paths matching known infrastructure prefixes
   (`/swagger`, `/health`, `/metrics`, etc.) skip all context logic entirely.

2. **Resolve actor** — attempts to extract the `oid` claim from the JWT:
   - JWT present and valid `oid` → `ActorType.User`, `actorId = user:{guid}`
   - JWT present but no valid `oid` → **401 Unauthorized**
   - No JWT + endpoint has `[AllowAnonymous]` → `ActorType.External`,
     `actorId = external:{clientIp}`
   - No JWT + endpoint requires authentication → **401 Unauthorized**

3. **Resolve tenant** from `x-tenant-id` header:
   - `ActorType.User` without a valid tenant → **400 Bad Request**
   - `ActorType.External` without a tenant → uses `SystemConstants.SystemTenantId`
     as fallback; the endpoint may enforce stricter requirements

4. **Resolve** `CorrelationId` from `x-correlation-id` (generates a new
   `Guid` if absent or empty) and `CausationId` from `x-causation-id`
   (optional; identifies the prior event that triggered this request).

5. **Resolve optional scope headers** (`x-partition-id`, `x-domain-id`),
   treating absent or `Guid.Empty` values as `null`.

6. **Open structured log scope** with all resolved values so every log
   line in the request carries the full context automatically.

7. **Build** the `ExecutionContext`.

8. **Validate scope authorization** via `IAuthorizationService.ValidateScopeAsync`
   — only for `ActorType.User`. External actors skip this step since they
   have no identity in the platform yet. → **403 Forbidden** on failure.

9. **Set** the context via `IExecutionContextService.SetExecutionContext`.

10. **Add** `x-correlation-id` to the response headers.

11. **Clear** the context in the `finally` block after the pipeline completes.

---

### 📍 2. Storage

```csharp
AsyncLocal<ExecutionContext?>
```

Guarantees:

* **Request isolation** — each request has its own independent context
* **Concurrent execution safety** — no shared state between parallel requests
* **Async/await propagation** — context flows naturally across `await` boundaries

`SetExecutionContext` throws `InvalidOperationException` if called more than
once within the same async context, preventing accidental overwrites.

`OverrideExecutionContext` bypasses that guard and is reserved exclusively
for infrastructure code that reconstructs context from external metadata
(e.g. KurrentDB subscription handlers). It must never be called from
application or domain code.

---

## 🧾 Integrated logging

A structured scope is opened at the start of every request:

```csharp
logger.BeginScope(new Dictionary<string, object?>
{
    ["CorrelationId"] = correlationId,
    ["ActorId"]       = actorId,
    ["ActorType"]     = actorType.Name,
    ["Executor"]      = executor,
    ["ExecutorType"]  = executorType.Name,
    ["TenantId"]      = effectiveTenantId,
    ["PartitionId"]   = partitionId,   // omitted by Serilog when null
    ["DomainId"]      = domainId,      // omitted by Serilog when null
})
```

---

## 🔁 Context propagation

### Outgoing HTTP

Propagate via a `DelegatingHandler`:

```
x-correlation-id
x-causation-id
x-tenant-id
x-partition-id
x-domain-id
```

### Messaging (MassTransit / RabbitMQ)

Include in message metadata:

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

### KurrentDB event metadata

Stored in `EventStoreMetadata` alongside the W3C TraceContext:

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

The `ExecutionContext` is reconstructed from this metadata by the
KurrentDB subscription handler for every consumed event.

---

## 🚫 Design decisions

### ❌ Do not allow unauthenticated access without explicit opt-in

Endpoints that accept `External` actors must be marked `[AllowAnonymous]`.
The middleware returns 401 for any unauthenticated request to an endpoint
that does not carry that attribute — even if the endpoint is not otherwise
protected by an authorization policy.

### ❌ Do not use Actor as Executor

Prevents audit errors where a worker bug appears to be the user's fault.

### ❌ Do not validate inside ExecutionContextService

| Component  | Responsibility              |
|------------|-----------------------------|
| Middleware | Construction + validation   |
| Service    | Storage only                |

### ❌ Do not use plain enums

`SmartEnum` eliminates magic strings, is extensible without breaking
changes, and serializes cleanly.

---

## ✅ Conventions

### Headers

| Header             | Required for User | Required for External   |
|--------------------|-------------------|-------------------------|
| `x-tenant-id`      | ✅                | optional (system fallback)|
| `x-correlation-id` | optional          | optional                |
| `x-causation-id`   | optional          | optional                |
| `x-partition-id`   | optional          | optional                |
| `x-domain-id`      | optional          | optional                |

### Validation rules

| Field           | Rule                                          |
|-----------------|-----------------------------------------------|
| `ActorId`       | Non-empty, must match a known prefix pattern  |
| `CorrelationId` | Non-empty `Guid`                              |
| `TenantId`      | Non-empty `Guid` (system GUID for External)   |
| `PartitionId`   | `null` or non-empty `Guid`                    |
| `DomainId`      | `null` or non-empty `Guid`                    |

---

## 🧪 Full examples

### Authenticated user request

```
POST /showtimes
Headers:
  Authorization:    Bearer {jwt with oid claim}
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

### Unauthenticated external request (e.g. registration)

```
POST /auth/register   [AllowAnonymous]
Headers:
  x-correlation-id: 3333...
  (no Authorization header, no x-tenant-id)
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

## 🧠 Mental model

| Concept           | Question it answers                      |
|-------------------|------------------------------------------|
| **Actor**         | Who wanted this to happen?               |
| **Executor**      | Who is actually running it?              |
| **CorrelationId** | What is the full chain of events?        |
| **CausationId**   | What specific event triggered this step? |
| **Scope**         | In which business context does it run?   |

---

## 🚀 Benefits

* Real and reliable auditability across sync and async flows
* Straightforward distributed debugging via correlation and causation
* Complete observability: structured logs + OTel traces linked end-to-end
* Decoupling between services through consistent header contracts
* Ready for event sourcing: context round-trips through KurrentDB metadata