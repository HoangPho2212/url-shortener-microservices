# Design Log #0001 - User Registration and JWT Unit Tests

## Background
The `UserManagement.Api` project has been migrated to MongoDB. We need robust unit tests to verify:
1. User registration logic (password hashing, email uniqueness, database insertion).
2. JWT generation logic (claim presence, signature validity, expiration).

## Problem
*   `AuthController` currently depends directly on `IMongoCollection<User>`.
*   `TokenService` uses `IConfiguration` for JWT settings.
*   Existing tests in `UserManagement.Api.Tests` need updating or replacement to ensure high coverage of business logic without relying on a real database.

## Questions and Answers
*   **Q: Should we use a real MongoDB for unit tests?**
    *   A: No. We will mock `IMongoCollection<User>` and `IAsyncCursor` to isolate business logic. Integration tests already cover the database interaction.
*   **Q: What library should we use for mocking?**
    *   A: `Moq` is already the industry standard and likely compatible with the existing test project.

## Design

### AuthController Unit Tests
*   **Target**: `UserManagement.Api.Controllers.AuthController.Register`
*   **Mocks**: `IMongoCollection<User>`, `ITokenService`.
*   **Scenarios**:
    *   ✅ Successful registration returns `CreatedAtAction`.
    *   ❌ Registration with existing email returns `BadRequest`.

### TokenService Unit Tests
*   **Target**: `UserManagement.Api.Services.TokenService.CreateToken`
*   **Mocks**: `IConfiguration`.
*   **Scenarios**:
    *   ✅ Token contains `sub`, `email`, and `unique_name` claims.
    *   ✅ Token is signed with the correct key.

## Implementation Plan

### Phase 1: Infrastructure
1.  Install `Moq` in `UserManagement.Api.Tests`.
2.  Create `UnitTests` directory if not present.

### Phase 2: TokenService Tests
1.  Update `UserManagement.Api.Tests/UnitTests/TokenServiceTests.cs`.
2.  Test `CreateToken` with various user profiles.

### Phase 3: AuthController Tests
1.  Create `UserManagement.Api.Tests/UnitTests/AuthControllerTests.cs`.
2.  Implement mock setup for `IMongoCollection<User>.Find().AnyAsync()`.
3.  Implement mock setup for `IMongoCollection<User>.InsertOneAsync()`.

## Examples

### ✅ Mocking MongoDB Find
```csharp
var mockCollection = new Mock<IMongoCollection<User>>();
var mockCursor = new Mock<IAsyncCursor<User>>();

// Setup for AnyAsync/FirstOrDefaultAsync
mockCollection.Setup(c => c.FindAsync(
    It.IsAny<FilterDefinition<User>>(),
    It.IsAny<FindOptions<User, User>>(),
    It.IsAny<CancellationToken>()))
    .ReturnsAsync(mockCursor.Object);
```

### ❌ Hardcoding Secrets in Tests
```csharp
// Use IConfiguration mock instead of real appsettings
var configMock = new Mock<IConfiguration>();
configMock.Setup(x => x["Jwt:Key"]).Returns("too-short-key"); // Avoid this
```

## Trade-offs
*   **Mocking MongoDB**: It's verbose and complex due to the fluent interface and extension methods. 
*   **Alternative**: Use an in-memory MongoDB driver, but Moq is more standard for pure unit tests.

## Implementation Results
*   **Infrastructure**: Installed `Moq` NuGet package.
*   **Architecture Refactoring**: Introduced `IUserRepository` and `UserRepository` to abstract `IMongoCollection<User>`. This was necessary because MongoDB's extension methods (like `.Find()`) cannot be mocked directly by Moq.
*   **TokenService Tests**: Updated existing tests to verify JWT claims and issuer.
*   **AuthController Tests**: Implemented 4 new unit tests covering registration (success/failure) and login (success/failure) using the new repository mock.
*   **Test Results**: 5/5 unit tests passing.

## Deviations from Original Design
*   **Introduction of Repository Pattern**: The original design planned to mock `IMongoCollection` directly. However, due to the heavy use of extension methods in the MongoDB C# driver, which `Moq` does not support, I refactored the controllers to use a repository pattern. This improved code testability and followed better architectural principles.
*   **Deleted Obsolete Integration Logic**: Cleaned up the `CustomWebApplicationFactory` to use MongoDB configuration instead of the defunct EF Core In-Memory database.
