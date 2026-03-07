# Design Log: 0002-testing-user-management-api

## Context & Intent
Verify the correctness of the `UserManagement.Api` implementation. This ensures that registration, login, JWT issuance, and protected CRUD operations work as expected and provides a safety net for future changes.

## Problem Statement
We need to ensure:
- Users can register and passwords are hashed securely.
- Users can log in and receive a valid JWT.
- Protected endpoints reject requests without a valid token.
- Protected endpoints accept requests with a valid token and perform CRUD correctly.

## Proposed Design

### Tech Stack
- **Test Framework:** xUnit
- **Assertion Library:** FluentAssertions
- **Integration Testing:** `Microsoft.AspNetCore.Mvc.Testing` (uses `WebApplicationFactory`)
- **Database for Tests:** `Microsoft.EntityFrameworkCore.InMemory`

### Project Structure
- `UserManagement.Api.Tests/`
    - `UnitTests/`
        - `TokenServiceTests.cs`: Verifies JWT generation logic.
    - `IntegrationTests/`
        - `CustomWebApplicationFactory.cs`: Setup for overriding DbContext and Environment.
        - `AuthControllerTests.cs`: Tests `/api/auth/register` and `/api/auth/login`.
        - `UsersControllerTests.cs`: Tests protected `/api/users` endpoints.

### Test Cases
1.  **Auth Integration:**
    - Register a valid user -> Expect 201 Created.
    - Login with valid credentials -> Expect 200 OK + JWT Token.
    - Login with invalid credentials -> Expect 401 Unauthorized.
2.  **Users Integration (Security):**
    - Access `GET /api/users` without token -> Expect 401 Unauthorized.
    - Access `GET /api/users` with valid token -> Expect 200 OK.
3.  **Token Unit:**
    - `CreateToken` returns a string that can be decoded to show correct claims.

## Questions & Gaps
- **Database Conflict:** Encountered an issue where both Npgsql and InMemory providers were registered. Fixed by adding an environment check in `Program.cs`.

## Implementation Results
- Created `UserManagement.Api.Tests` project and added to solution.
- Added necessary NuGet packages (`FluentAssertions`, `Mvc.Testing`, `EFCore.InMemory`).
- Implemented unit and integration tests covering all major requirements.
- Updated `Program.cs` to support "Test" environment for DB registration.

### Verification
- `dotnet test` results: **Total: 7, Passed: 7, Failed: 0**.
- All authentication and authorization logic verified.
