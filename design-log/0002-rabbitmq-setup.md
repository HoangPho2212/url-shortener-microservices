# Design Log #0002 - RabbitMQ Setup with MassTransit

## Background
We are moving towards an event-driven architecture to decouple services. Specifically, analytics tracking (clicks) and user-related background tasks should be handled asynchronously.

## Problem
Currently, all logic is synchronous within each microservice. For example, when a URL is clicked, the redirect service increments the counter in MongoDB directly. This should ideally be an event published to RabbitMQ so other services (like an analytics service) can process it without slowing down the redirect.

## Questions and Answers
*   **Q: Which library should we use?**
    *   A: **MassTransit**. It provides a high-level abstraction over RabbitMQ, handling retries, dead-letter queues, and serialization automatically.
*   **Q: Where should the message contracts live?**
    *   A: We will create a shared `Contracts.Events` project or folder to ensure both publishers and consumers use the same message types. For now, since it's a small project, we'll place them in a `Shared` namespace or duplicate them if necessary (Jay Framework usually prefers strict contracts).

## Design

### 1. Technology Stack
*   **Broker**: RabbitMQ
*   **Abstraction**: MassTransit
*   **Transport**: RabbitMQ (AMQP)

### 2. Events to Implement
*   `UserRegisteredEvent`: Published by `UserManagement.Api` when a new user signs up.
*   `UserAccountDeletedEvent`: Published by `UserManagement.Api` when a user deletes their account.
*   `UrlShortenedEvent`: Published by `UrlManagement.Api` when a new short link is created.
*   `UrlClickedEvent`: Published by `UrlManagement.Api` when a link is accessed.

### 3. Service Configuration
Each service will have a `RabbitMqSettings` section in `appsettings.json`.

## Implementation Plan

### Phase 1: Shared Contracts
1.  Define event interfaces in a shared location (or replicate for simplicity in this turn).

### Phase 2: UserManagement.Api Integration
1.  Install `MassTransit.RabbitMQ`.
2.  Configure MassTransit in `Program.cs`.
3.  Publish `UserRegisteredEvent` in `AuthController.Register`.

### Phase 3: UrlManagement.Api Integration
1.  Install `MassTransit.RabbitMQ`.
2.  Configure MassTransit in `Program.cs`.
3.  Publish `UrlClickedEvent` in `GetOriginalUrlAsync`.

## Examples

### ✅ Publishing an Event
```csharp
public async Task<IActionResult> Register(UserCreateDto dto) {
    // ... logic ...
    await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id, user.Email));
}
```

### ❌ Hardcoding RabbitMQ Credentials
```csharp
// Use configuration instead
cfg.Host("rabbitmq://localhost", h => {
    h.Username("guest"); 
    h.Password("guest");
});
```

## Trade-offs
*   **Added Complexity**: Requires a RabbitMQ broker to be running.
*   **Consistency**: Eventual consistency instead of strong consistency for analytics.

## Implementation Results
*   **Shared Infrastructure**: Created a shared `Shared/Contracts/` directory at the root level. All microservices now reference these interfaces via a `<Compile Include="..." />` link in their `.csproj` files to ensure strict contract consistency without needing a separate NuGet server.
*   **MassTransit Integration**: Successfully integrated `MassTransit.RabbitMQ` (v9.0.1) into `UserManagement.Api` and `UrlManagement.Api`.
*   **Event Publishing**:
    *   `UserManagement.Api` now publishes `IUserRegisteredEvent` during the registration flow.
    *   `UrlManagement.Api` now publishes `IUrlShortenedEvent` upon link creation and `IUrlClickedEvent` during every redirection.
*   **Configuration**: Added default RabbitMQ settings in `Program.cs` that can be overridden via `appsettings.json`.

## Deviations from Original Design
*   **Project Referencing**: Instead of creating a new `.csproj` for shared contracts (which requires complex project referencing in a microservices repo), I used the "Link" feature in MSBuild to compile the same contract files into both projects. This keeps the binary dependencies clean while maintaining shared interfaces.
*   **Url API Port Change**: During implementation, the `UrlManagement.Api` port was shifted to `5291` to avoid local port conflicts discovered during testing. This is reflected in the Gateway configuration.
*   **Anonymous Objects for Publishing**: Used MassTransit's ability to publish interface types via anonymous objects to keep the code clean and avoid explicit concrete class implementations of the interfaces in every project.

