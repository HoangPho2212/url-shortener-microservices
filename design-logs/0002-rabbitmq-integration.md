# Design Log: 0002-rabbitmq-integration-and-testing

## Context & Intent
Integrate RabbitMQ for asynchronous event publishing between microservices and add comprehensive integration tests to verify both database operations and message delivery.

## Problem Statement
The current system lacks:
1.  Asynchronous communication between `UserManagement.Api` and `UrlManagement.Api`.
2.  A way to notify other services when a new user is registered or a URL is shortened.
3.  Integration tests that verify the atomicity or correctness of database operations combined with event publishing.

## Proposed Design

### 1. Messaging Framework
- **MassTransit** with RabbitMQ as the transport.
- Use `MassTransit.TestFramework` or `InMemoryBus` for integration tests to avoid requiring a live RabbitMQ instance during test execution.

### 2. Events
- **`UserRegisteredEvent`** (published by `UserManagement.Api`)
  - `UserId` (Guid)
  - `Email` (string)
  - `Username` (string)
- **`UrlShortenedEvent`** (published by `UrlManagement.Api`)
  - `Id` (Guid)
  - `OriginalUrl` (string)
  - `ShortCode` (string)
  - `UserId` (Guid?)

### 3. Integration Testing Strategy
- Use `WebApplicationFactory` for both services.
- Replace the live RabbitMQ bus with MassTransit's **In-Memory Bus** in the test environment.
- Use a `Harness` (MassTransit's test harness) to intercept and verify that messages are actually published to the bus.
- Verify that database records are correctly persisted before or during the event publication.

### 4. Implementation Plan

#### Phase 1: Package Installation
- Add `MassTransit.RabbitMQ` and `MassTransit.AspNetCore` to both API projects.
- Add `MassTransit` test dependencies to test projects.

#### Phase 2: UserManagement.Api Integration
- Define `UserRegisteredEvent`.
- Register MassTransit in `Program.cs`.
- Update `AuthController.Register` to publish `UserRegisteredEvent`.
- Update `UserManagement.Api.Tests` to verify both DB entry and event publication.

#### Phase 3: UrlManagement.Api Integration
- Define `UrlShortenedEvent`.
- Register MassTransit in `Program.cs`.
- Update `UrlsController.Shorten` to publish `UrlShortenedEvent`.
- Update `UrlManagement.Api.Tests` to verify both DB entry and event publication.

## Questions & Gaps
1.  **RabbitMQ Connection:** Should we use default local settings or environment variables? (Default to local/docker-compose).
2.  **Outbox Pattern:** Should we implement the Transactional Outbox pattern to ensure events are only published if the DB transaction succeeds? (Highly recommended for "ensuring they work correctly together").

## Implementation Results
- **Phase 1:** Installed MassTransit v8.2.5 and RabbitMQ transport.
- **Phase 2:** `UserRegisteredEvent` implemented and published from `AuthController.Register`.
- **Phase 3:** `UrlShortenedEvent` implemented and published from `UrlsController.Shorten`.
- **Integration Tests:**
  - `UserManagement.Api.Tests` updated with `ITestHarness` to verify `UserRegisteredEvent`.
  - `UrlManagement.Api.Tests` updated with `ITestHarness` to verify `UrlShortenedEvent`.
  - All tests passed (10/10 in UserManagement, 4/4 in UrlManagement).

### Deviations
- Upgraded MassTransit to v8.2.5 to support `AddMassTransitTestHarness` and `ITestHarness`.
- Used local loopback bus for integration tests via `AddMassTransitTestHarness()`.

### Verification
- Ran `dotnet test` for both projects: All tests passed.
