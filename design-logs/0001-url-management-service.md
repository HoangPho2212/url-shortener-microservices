# Design Log: 0001-url-management-service

## Context & Intent
Create a new microservice, `UrlManagement.Api`, to handle URL shortening, storage, and redirection. This service will operate independently of the `UserManagement.Api` but will integrate with its JWT-based authentication for user-owned URLs.

## Problem Statement
The system needs a way for users to:
1.  Shorten long URLs into unique, manageable short codes.
2.  Persist these mappings in a dedicated PostgreSQL database.
3.  Retrieve original URLs for redirection based on the short code.
4.  Optionally track which user created which URL.

## Proposed Design

### 1. Database Schema (PostgreSQL)
**Table: ShortUrls**
- `Id` (Guid, PK)
- `OriginalUrl` (string, required, URL format)
- `ShortCode` (string, required, unique index, 6-10 characters)
- `CreatedAt` (DateTime, default: now)
- `UserId` (Guid?, optional, indexed for lookup)

### 2. API Signatures
- `POST /api/urls/shorten`
  - Body: `{ "url": "https://example.com" }`
  - Auth: Optional (if provided, link to `UserId`)
  - Response: `{ "shortUrl": "https://short.ly/abc123" }`
- `GET /api/urls/{shortCode}`
  - Response: Redirect to `OriginalUrl` (302) or 404.
- `GET /api/urls/my-urls`
  - Auth: Required
  - Response: List of user's shortened URLs.

### 3. File Paths
- `UrlManagement.Api/Models/ShortUrl.cs`
- `UrlManagement.Api/Data/UrlDbContext.cs`
- `UrlManagement.Api/Controllers/UrlsController.cs`
- `UrlManagement.Api/Services/UrlShorteningService.cs`

### 4. Implementation Plan
- **Phase 1: Project Setup**
  - Create ASP.NET Core Web API project.
  - Add to `UrlShortener.sln`.
  - Install dependencies: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`.
- **Phase 2: Data Layer**
  - Define `ShortUrl` model.
  - Configure `UrlDbContext`.
  - Add initial EF Core migration.
- **Phase 3: Core Logic**
  - Implement a `UrlShorteningService` to generate unique short codes (e.g., using Base62 or a hash-based approach).
- **Phase 4: API Implementation**
  - Create `UrlsController` with `Shorten`, `Redirect`, and `GetMyUrls` endpoints.
  - Configure JWT Authentication (aligned with `UserManagement.Api`).
- **Phase 5: Validation & Testing**
  - Integration tests for shortening and redirection.

## Questions & Gaps
1.  **Short Code Generation:** Do we want a random 6-8 character alphanumeric string, or something deterministic?
2.  **Domain for Short URLs:** What domain should we use for the generated short URLs in the response?
3.  **Analytics:** Should we track clicks/hits in this phase or a later one?
4.  **Database Connection String:** Should I use a separate container or just a separate database name on the same PostgreSQL instance?

## Implementation Results
- **Phase 1-4:** Completed. `UrlManagement.Api` created with controllers, services, and EF Core models.
- **Phase 5:** Integration tests implemented and passing (4/4 passing).

### Deviations
- Added `Swashbuckle.AspNetCore` for Swagger UI and JWT support.
- Configured `HttpClient` in tests to disable auto-redirects for status code validation.
- Trailing slashes in redirected URLs are handled by the test expectations.

### Verification
- Ran `dotnet test UrlManagement.Api.Tests`: All 4 tests passed.
- EF Core migration `InitialCreate` generated.
