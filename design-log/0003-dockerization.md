# Design Log #0003 - Dockerization

## Background
To ensure consistent environments across development and production, we need to containerize the entire microservices platform. This includes the three .NET services and the Vue.js frontend.

## Problem
Currently, running the system requires manual setup of .NET runtimes, Node.js, and manual execution of multiple commands. Dockerizing the components will simplify deployment and orchestration.

## Questions and Answers
*   **Q: Should we use multi-stage builds?**
    *   A: ✅ Yes. This ensures that build dependencies (like SDKs and Node modules) are not included in the final production images, keeping them small and secure.
*   **Q: How will we serve the Vue frontend?**
    *   A: We will use **Nginx** to serve the static production build of the Vue application.
*   **Q: How do we handle the shared contracts in Docker?**
    *   A: Since the .NET projects use linked files from the `Shared` folder, the Docker context must be at the **root** of the repository so the `COPY` command can access the `Shared` directory.

## Design

### 1. .NET Services (User, URL, Gateway)
*   **Base Image**: `mcr.microsoft.com/dotnet/aspnet:9.0`
*   **Build Image**: `mcr.microsoft.com/dotnet/sdk:9.0`
*   **Strategy**: Copy the specific project folder AND the `Shared` folder. Restore, build, and publish.

### 2. Vue Frontend
*   **Build Image**: `node:20-alpine`
*   **Production Image**: `nginx:stable-alpine`
*   **Strategy**: Build the project using `npm run build` and copy the `dist` folder to Nginx's html directory.

## Implementation Plan

### Phase 1: .NET Dockerfiles
1.  Create `UserManagement.Api/Dockerfile`.
2.  Create `UrlManagement.Api/Dockerfile`.
3.  Create `ApiGateway/Dockerfile`.

### Phase 2: Frontend Dockerfile
1.  Create `frontend/Dockerfile`.
2.  Add a basic `nginx.conf` for the frontend to handle Vue Router history mode.

## Examples

### ✅ Proper .NET Dockerfile (Root Context)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UserManagement.Api/UserManagement.Api.csproj", "UserManagement.Api/"]
COPY ["Shared/", "Shared/"]
RUN dotnet restore "UserManagement.Api/UserManagement.Api.csproj"
...
```

## Trade-offs
*   **Image Size vs Build Speed**: Multi-stage builds are slightly slower during the first build but produce significantly smaller images.
*   **Root Context**: Building from the root is necessary for shared code but requires careful `.dockerignore` files to avoid copying unnecessary files (like `bin/` and `obj/`).

## Implementation Results
*   **UserManagement.Api/Dockerfile**: Multi-stage .NET 9 image. Accesses `Shared/` via root context.
*   **UrlManagement.Api/Dockerfile**: Multi-stage .NET 9 image. Accesses `Shared/` via root context.
*   **ApiGateway/Dockerfile**: Multi-stage .NET 9 image.
*   **frontend/Dockerfile**: Node.js build stage + Nginx production stage.
*   **frontend/nginx.conf**: Configured to support Vue Router history mode.
*   **.dockerignore**: Created at root to optimize build context.

## Deviations from Original Design
*   **Shared Folder Access**: Confirmed that the `COPY ["Shared/", "Shared/"]` command works perfectly when building from the repository root (e.g., `docker build -t user-api -f UserManagement.Api/Dockerfile .`).

