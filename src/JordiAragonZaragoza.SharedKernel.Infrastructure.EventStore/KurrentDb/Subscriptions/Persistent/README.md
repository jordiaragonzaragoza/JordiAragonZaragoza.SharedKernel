# Persistent Subscription to `$all`

## Overview

`KurrentDbAllStreamPersistentSubscription` implements a persistent subscription to the `$all` stream of KurrentDB using the native consumer group pattern.

**Key differences compared to a catch-up subscription:**

|  | CatchUp | Persistent |
| --- | --- | --- |
| Checkpoint state | Client (file/DB) | Server (KurrentDB) |
| Distribution among instances | No — each instance reads everything | Yes — KurrentDB distributes automatically |
| ACK/NACK | Not applicable | Mandatory — controls checkpoint progression |
| Dead-letter (parking) | No | Yes — automatic after `MaxRetryCount` NACKs |
| Use case | Projector (single instance) | Reactor worker (horizontally scalable) |

---

## Internal Architecture

```
AllStreamPersistentSubscriptionBackgroundWorker (BackgroundService)
│
├── ExecuteAsync(stoppingToken)
│   ├── SubscribeToAllAsync()          → connects and returns immediately
│   └── Task.Delay(Infinite, token)    → keeps the worker alive
│
KurrentDbAllStreamPersistentSubscription
│
├── SubscribeToAllAsync()
│   └── ConnectToGroupAsync()
│       └── SubscribeToGroupAsync()    → attempts to subscribe
│           ├── PersistentSubscriptionNotFoundException
│           │   ├── CreateSubscriptionGroupAsync()  → creates the group on the server
│           │   └── SubscribeToAllAsync() (retry)
│           └── AlreadyExistsException              → race condition resolved, continues
│
├── HandleEventCallbackAsync()         → per-event processing (server callback)
│   ├── IsSystemEvent → silent ACK
│   ├── IsEventWithEmptyData → silent ACK
│   ├── SerializerHelper.Deserialize()
│   │   ├── Error + IgnoreDeserializationErrors=true → ACK + Warning log
│   │   └── Error + IgnoreDeserializationErrors=false → throw → NACK + retry
│   ├── unitOfWork.ExecuteInTransactionAsync()
│   │   └── eventBus.PublishAsync(domainEvent)
│   ├── ACK → advances checkpoint on the server
│   ├── OperationCanceledException (shutdown) → no ACK, no NACK, informative log
│   └── Exception → NACK(Retry) → server retries up to MaxRetryCount → Park
│
└── HandleSubscriptionDropped()        → server callback when connection drops
    ├── Disposed / stoppingToken cancelled → do not reconnect
    └── Any other reason → ReconnectWithBackoffAsync() in background Task.Run
        └── SubscribeToGroupAsync() with exponential backoff (2s → 4s → ... → 60s cap)

```

---

## Worker Lifecycle

The `BackgroundService` clearly separates two responsibilities:

1. **`SubscribeToAllAsync`** — connects to KurrentDB and registers callbacks. It returns as soon as the subscription is established. It does not block.
2. **`Task.Delay(Timeout.Infinite, stoppingToken)`** in the worker — keeps `ExecuteAsync` alive. When the host signals a shutdown, the token is canceled, the `Delay` throws an `OperationCanceledException`, and `ExecuteAsync` returns cleanly.

Events arrive via the `HandleEventCallbackAsync` callback, which KurrentDB invokes on its own gRPC channel, independently of the `Delay`.

---

## Failure Management

### Failure during event processing

```
Event fails in handler
    → NACK(Retry)
    → KurrentDB retries (up to MaxRetryCount times)
    → If MaxRetryCount is exceeded → Automatic park in:
       $persistentsubscription-{groupName}-parked

```

Parking is native to the server. No client-side logic is required. Parked events can be inspected and replayed from the KurrentDB UI or via the API.

### Connection failure (subscription drop)

```
HandleSubscriptionDropped(reason, exception)
    │
    ├── reason == Disposed → host stopping, do not reconnect
    ├── stoppingToken.IsCancellationRequested → do not reconnect
    └── any other reason:
        Task.Run → ReconnectWithBackoffAsync()
            ├── ServerError: initial delay 2s
            └── Others: initial delay 10s
            Backoff: delay * 2 per failed attempt, capped at 60s
            Loop: while (isRunning && isDropped && !cancelled)

```

Reconnection reuses `SubscribeToGroupAsync`, which includes automatic group creation if necessary.

### Host shutdown during processing

If the `stoppingToken` is canceled while an event is being processed, the `OperationCanceledException` is explicitly caught **without performing a NACK**. The server detects the disconnection and redelivers the event to the next available consumer in the group.

---

## Configuration

### `KurrentDbAllStreamPersistentSubscriptionOptions`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `GroupName` | `string` | `"all-stream-group"` | Name of the consumer group in KurrentDB. All instances of the same worker must use the same name. |
| `BufferSize` | `int` | `10` | Maximum number of concurrent in-flight events that the client accepts from the server. |
| `MaxRetryCount` | `int` | `5` | Number of server retries before parking the event in the dead-letter stream. Configured on the server upon group creation, not on the client. |
| `IgnoreDeserializationErrors` | `bool` | `true` | If `true`, events that fail deserialization are ACKed and skipped with a warning. If `false`, they are NACKed and enter the retry/park cycle. Useful as `false` to detect schema regressions in stable production. |
| `ResolveLinkTos` | `bool` | `false` | If `true`, resolves link events to the original event. |
| `FilterOptions` | `SubscriptionFilterOptions?` | `null` | Server-side filter applied when creating the group. Does not affect already existing groups. |
| `Credentials` | `UserCredentials?` | `null` | Credentials for the KurrentDB client. If `null`, it uses those of the client registered in DI. |

> **Important:** `MaxRetryCount` and `FilterOptions` only take effect during group **creation**. If the group already exists in KurrentDB, modifying these options will not update it. To change them on an existing group, it must be deleted and recreated.

---

## DI Registration

### Default configuration

```csharp
// Program.cs — Reactor Worker
builder.Services
    .AddSharedKernelInfrastructureKurrentDbAllStreamPersistentSubscription();

```

### Custom configuration

Create an extension in your project if you need options other than the defaults:

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

## Full Example — Reactor Worker

```csharp
// Worker.Reactor/Program.cs
public static async Task Main(string[] args)
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    var configuration = builder.Configuration;

    builder.AddInfrastructure();
    builder.AddInfrastructureKurrentDbClient();

    // Reactor-specific registrations
    builder.Services
        .AddDomain()
        .AddApplication()
        .AddApplicationValidators()
        .AddApplicationCommandHandlers()
        .AddApplicationPolicies(configuration)       // registers policies/reactors
        .AddInfrastructureEventStoreRepositories();

    // SharedKernel registrations
    builder.Services
        .AddSharedKernelDomain()
        .AddSharedKernelApplicationCommandBus()
        .AddSharedKernelInfrastructureKurrentDbBusiness()
        .AddSharedKernelInfrastructure()             // includes IExecutionContextService (AsyncLocal)
        .AddSharedKernelInfrastructureReactorBus()   // IEventBus + ICommandBus
        .AddSharedKernelInfrastructureKurrentDbAllStreamPersistentSubscription();

    IHost app = builder.Build();
    await app.RunAsync();
}

```

---

## Policy Example (Reactor)

Policies receive events through the in-memory bus. The complete flow is:

```
KurrentDB $all
    → HandleEventCallbackAsync (deserializes + sets ExecutionContext)
    → unitOfWork.ExecuteInTransactionAsync
        → eventBus.PublishAsync(domainEvent)
            → MediatR → ScheduleShowtimeInAuditoriumPolicy.HandleAsync
                → commandBus.SendAsync(new ScheduleShowtimeInAuditoriumCommand(...))
                    → MediatR → command handler → aggregate → event saved in KurrentDB
    → ACK to the server

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

## Horizontal Scaling

All worker instances must use the same `GroupName`. KurrentDB automatically distributes the group's events among connected consumers (round-robin by default, configurable via `ConsumerStrategy`).

```yaml
# kubernetes/reactor-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cinema-reactor-worker
spec:
  replicas: 3          # KurrentDB distributes events among the 3 instances
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
# Scale on the fly — KurrentDB automatically redistributes
kubectl scale deployment cinema-reactor-worker --replicas=5

```

> **Startup race condition:** If multiple instances start simultaneously and the group does not exist, they may attempt to create it at the same time. The implementation handles this: the first one to create the group succeeds; the others receive an `RpcException(AlreadyExists)` which is silently caught, and all proceed to subscribe to the already existing group.

---

## Corner Cases

### The group already exists with a different configuration

If you change `MaxRetryCount` or `FilterOptions` in the options but the group already exists in KurrentDB, the new values **are not applied**. The server maintains the group's original configuration.

**Solution:** Delete the group from the KurrentDB UI or via the API and restart the worker. The group will be recreated with the new configuration.

### Permanently unprocessable event

If a handler has a bug that causes the same event to always fail:

1. The server retries `MaxRetryCount` times.
2. Upon exceeding the limit, the event is parked in `$persistentsubscription-{groupName}-parked`.
3. The worker continues processing the next event — it does not block.
4. Fix the bug, redeploy, and replay the parked event from the UI.

### Disconnection during processing

If the worker loses connection while processing an event (before the ACK):

* KurrentDB detects the disconnection.
* The event is re-queued for another consumer in the group (or for the same one when it reconnects).
* The event may be processed twice if the handler already completed its logic but did not manage to send the ACK. Handlers must be **idempotent**.

### Shutdown during processing

If the host stops (`Ctrl+C`, SIGTERM) while an event is being processed:

* `stoppingToken` is canceled.
* `OperationCanceledException` is caught without sending a NACK.
* The server redelivers the event to the next available consumer.
* There is no event loss, but double processing may occur if the handler has already completed its work. Idempotence is mandatory.

---

## Logging

### Relevant levels

| Level | What it logs |
| --- | --- |
| `Information` | Startup, established connection, reconnections, execution context fallback |
| `Warning` | Subscription drop, ignored deserialization errors |
| `Error` | Event processing failures, NACK failures, reconnection errors |
| `Debug` | Skipped system events, processed and ACKed events, missing metadata |

```json
{
  "Logging": {
    "LogLevel": {
      "JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Subscriptions.Persistent": "Debug"
    }
  }
}

```

### Traces in Datadog / OpenTelemetry

Each processed event restores the `Activity` from the event metadata (`traceparent`/`tracestate` via `EventStoreActivityRestorer`), propagating the original distributed trace. Tags added to the span include `actor.id`, `correlation.id`, `causation.id`, `tenant.id`, and `event.occurred_at`.

---

## Monitoring from the KurrentDB UI

**Path:** `Persistent Subscriptions` → select the group

| Metric | Description |
| --- | --- |
| **Connected** | Number of worker instances currently connected to the group |
| **Last checkpoint** | Position of the last ACKed event — indicates progress |
| **In-flight** | Events delivered but not yet ACKed ($\leq$ `BufferSize`) |
| **Parked messages** | Events that exceeded `MaxRetryCount` — require manual attention |

**Rule of thumb:** If *Parked messages* grows, check the worker's error logs. If *Connected* is 0 and the worker should be active, there is a connectivity issue or the host did not start correctly.