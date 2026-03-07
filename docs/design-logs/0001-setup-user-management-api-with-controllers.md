# Design Log: 0001-setup-user-management-api-with-controllers

## Context & Intent
Establish the `UserManagement.Api` microservice using a traditional ASP.NET Core Controller-based architecture. This service handles user registration, JWT authentication, and profile management using PostgreSQL for persistence.

## Problem Statement
The system requires a scalable, maintainable, and well-structured identity management service. Using traditional Controllers provides a clear separation of concerns (Routing/HTTP vs Business Logic).

## Proposed Design

### Tech Stack
- **Framework:** .NET 9.0 (ASP.NET Core Web API with Controllers)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Auth:** JWT Bearer & BCrypt.Net-Next

### Project Structure
- `Models/`: `User.cs` entity.
- `DTOs/`: `UserCreateDto.cs`, `UserLoginDto.cs`, `AuthResponseDto.cs`.
- `Data/`: `UserDbContext.cs`.
- `Services/`: `TokenService.cs` (Implements `ITokenService`).
- `Controllers/`:
    - `AuthController.cs`: Handles `Register` and `Login`.
    - `UsersController.cs`: Handles CRUD for users (Protected with `[Authorize]`).

### API Endpoints
- **Auth:**
    - `POST /api/auth/register`
    - `POST /api/auth/login`
- **Users:**
    - `GET /api/users` (Protected)
    - `GET /api/users/{id}` (Protected)
    - `PUT /api/users/{id}` (Protected)
    - `DELETE /api/users/{id}` (Protected)

## Questions & Gaps
- **Validation:** Basic validation is implemented via EF configuration.

## Implementation Results
- Configured `Program.cs` to use `AddControllers()` and `MapControllers()`.
- Implemented `AuthController` for registration and login (with BCrypt hashing).
- Implemented `UsersController` with protected CRUD operations.
- Maintained existing models, DTOs, and services.

### Verification
- Project build successful.
- Initial migration (from previous step) remains compatible.
- Controllers correctly handle routing and dependency injection.
