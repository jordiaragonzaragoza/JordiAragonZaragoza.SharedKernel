# 🧠 ExecutionContext

## 📌 Purpose

The `ExecutionContext` is the central mechanism for:

* Propagating **identity and traceability information**
* Maintaining **consistent context** across layers
* Enabling **complete auditability**
* Facilitating **observability (logs, tracing, debugging)**

It is established **once per request / message** and accessed globally throughout its execution.

---

## 🧩 Key concepts

### 1. Actor (who originates the intent)

Represents **who initiated the action**, not who executes it technically.

```csharp
string ActorId      // Format: "{prefix}:{identifier}"
ActorType ActorType // SmartEnum: user | system | external
```

#### Examples:

| Case                          | ActorId                                     | ActorType |
| ----------------------------- | ------------------------------------------- | --------- |
| User                          | `user:123e4567-e89b-12d3-a456-426614174000` | user      |
| Worker reacting to an event   | `user:123...` (propagated)                  | user      |
| Batch job                     | `job:daily-cleanup`                         | system    |

> **Important:** The Actor **does not change throughout the flow**.

#### ActorId format

The `ActorId` must start with one of the following known prefixes followed by a non-empty identifier:

```
user:{guid}
service:{name}
job:{name}
```

This is enforced at construction time via `ExecutionContext.IsValidActorIdFormat(actorId)`.

---

### 2. Executor (who executes technically)

Represents the system that is actually running the action.

```csharp
string Executor           // e.g. "cinema-reservation-api-command"
ExecutorType ExecutorType // SmartEnum: service | worker | tool
```

#### Examples:

| Service | Executor                         | ExecutorType |
| ------- | -------------------------------- | ------------ |
| API     | `cinema-reservation-api-command` | service      |
| Worker  | `reservation-reactor-worker`     | worker       |

> This allows answering questions like:
> * "Who actually executed this?"
> * "Which service is failing?"

---

### 3. Correlation & Causation

```csharp
Guid  CorrelationId  // Required. Never empty.
Guid? CausationId    // Optional. Identifies the triggering event.
```

#### CorrelationId

* Identifies **the entire execution chain**
* Generated at the entry point (API Gateway typically)
* Always propagated via `x-correlation-id` header
* A new `Guid` is generated if the header is absent or empty

#### CausationId

* Indicates **which event or command caused this execution**
* Extracted from the `x-causation-id` header
* `null` for direct HTTP requests with no prior event

#### Flow example:

```
HTTP Request
  CorrelationId = A
  CausationId   = null

Event 1 (published)
  CorrelationId = A
  CausationId   = null

Event 2 (reaction to Event 1)
  CorrelationId = A
  CausationId   = Event1.Id
```

---

### 4. ScopeContext (business scope)

```csharp
Guid  TenantId     // Required. Never empty.
Guid? PartitionId  // Optional. Must not be Guid.Empty if provided.
Guid? DomainId     // Optional. Must not be Guid.Empty if provided.
```

Represents the **functional scope** where the action occurs.

#### Example:

* Tenant → company
* Partition → cinema
* Domain → screen

---

## ⚙️ How it works

### 📍 1. On HTTP entry

The `ExecutionContextMiddleware` runs once per request and performs the following steps in order:

1. **Resolves the actor** from the JWT (`oid` claim → `user:{guid}`, `ActorType.User`). Returns `401` if unauthenticated or the claim is missing.
2. **Validates the tenant** from the `x-tenant-id` header. Returns `400` if absent or `Guid.Empty`.
3. **Resolves** `CorrelationId` from `x-correlation-id` (generates a new one if absent) and `CausationId` from `x-causation-id` (optional).
4. **Opens a structured log scope** with all resolved values so every log line in the request carries full context automatically.
5. **Resolves optional scope headers** (`x-partition-id`, `x-domain-id`), treating absent or `Guid.Empty` values as `null`.
6. **Builds** the `ExecutionContext`.
7. **Validates scope authorization** via `IAuthorizationService.ValidateScopeAsync(...)`. Returns `403` on failure.
8. **Sets** the context via `IExecutionContextService.SetExecutionContext(...)`.
9. **Adds** `x-correlation-id` to the response headers.
10. **Clears** the context in the `finally` block after the pipeline completes.

---

### 📍 2. Storage

```csharp
AsyncLocal<ExecutionContext?>
```

Guarantees:

* **Request isolation** — each request has its own independent context
* **Concurrent execution safety** — no shared state between parallel requests
* **Async/await propagation** — context flows naturally across `await` boundaries

`SetExecutionContext` throws `InvalidOperationException` if called more than once within the same async context, preventing accidental overwrites.

---

## 🧾 Integrated logging

A structured scope is opened at the start of every request:

```csharp
logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["ActorId"]       = actorId,
    ["ActorType"]     = actorType.Name,
    ["Executor"]      = executor,
    ["ExecutorType"]  = executorType.Name,
    ["TenantId"]      = tenantId,
})
```

Result: every log entry within the request automatically includes:

```json
{
  "CorrelationId": "2222...",
  "ActorId":       "user:123...",
  "ActorType":     "user",
  "Executor":      "cinema-reservation-api-command",
  "ExecutorType":  "service",
  "TenantId":      "1111..."
}
```

---

## 🔁 Context propagation

### Outgoing HTTP

Must be propagated via headers using a `DelegatingHandler`:

```
x-correlation-id
x-causation-id
x-tenant-id
x-partition-id
x-domain-id
```

### Messaging (MassTransit / RabbitMQ)

Must be included in the message metadata:

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

The `ExecutionContext` is reconstructed from this metadata when the message is consumed. `ActorId` format is validated via `ExecutionContext.IsValidActorIdFormat(actorId)` before construction.

---

## 🚫 Design decisions

### ❌ Do not use Actor as Executor

Prevents audit errors:

> A bug in a worker would appear to be the user's fault.

### ❌ Do not validate inside ExecutionContextService

Separation of responsibilities:

| Component  | Responsibility               |
| ---------- | ---------------------------- |
| Middleware | Construction + validation    |
| Service    | Storage only                 |

### ❌ Do not use plain enums

`SmartEnum` is used because it:

* Eliminates magic strings
* Is extensible without breaking changes
* Serializes cleanly

---

## ✅ Conventions

### Headers

| Header            | Required |
| ----------------- | -------- |
| `x-tenant-id`     | ✅        |
| `x-correlation-id`| optional |
| `x-causation-id`  | optional |
| `x-partition-id`  | optional |
| `x-domain-id`     | optional |

### Validation rules

| Field         | Rule                                          |
| ------------- | --------------------------------------------- |
| `ActorId`     | Non-empty, must match a known prefix pattern  |
| `CorrelationId` | Non-empty `Guid`                            |
| `TenantId`    | Non-empty `Guid`                              |
| `PartitionId` | `null` or non-empty `Guid`                   |
| `DomainId`    | `null` or non-empty `Guid`                   |

---

## 🧪 Full example

### HTTP request

```
POST /showtimes
Headers:
  x-tenant-id:      1111...
  x-correlation-id: 2222...
```

### Resulting ExecutionContext

```json
{
  "ActorId":      "user:123...",
  "ActorType":    "user",
  "Executor":     "cinema-reservation-api-command",
  "ExecutorType": "service",
  "CorrelationId": "2222...",
  "CausationId":  null,
  "ScopeContext": {
    "TenantId":    "1111...",
    "PartitionId": null,
    "DomainId":    null
  }
}
```

---

## 🧠 Mental model

| Concept         | Question it answers                    |
| --------------- | -------------------------------------- |
| **Actor**       | Who wanted this to happen?             |
| **Executor**    | Who is actually running it?            |
| **CorrelationId** | What is the full chain of events?    |
| **CausationId** | What triggered this specific step?     |
| **Scope**       | In which business context does it run? |

---

## 🚀 Benefits

* Real and reliable auditability
* Straightforward distributed debugging
* Complete observability out of the box
* Decoupling between services
* Ready for event sourcing