# Design Log #0004 - Docker Orchestration

## Background
We have created Dockerfiles for individual services. Now we need to orchestrate them along with their infrastructure dependencies (MongoDB, Redis, RabbitMQ) using Docker Compose.

## Problem
*   Services need to communicate with each other and infrastructure.
*   Hardcoded `localhost` in `appsettings.json` and `ocelot.json` will not work inside Docker containers.
*   We need a single command to start the entire environment.

## Questions and Answers
*   **Q: How to handle port mappings?**
    *   A: We will keep the host ports consistent with the local development ports (5173, 5130, 5015, 5291) to avoid confusing the user.
*   **Q: How to handle Ocelot configuration for Docker?**
    *   A: We will create a `ocelot.Docker.json` or use environment variable overrides if supported. Since Ocelot makes it tricky to override specific Host/Port via env vars without complex structures, we will provide a `docker-compose.yml` that overrides the `ocelot.json` via a volume or environment variables if we use a specific Ocelot feature. Actually, the simplest way for this project is to use environment variables to override the connection strings.

## Design

### 1. Infrastructure Services
*   **mongodb**: `mongo:latest` on port 27017.
*   **redis**: `redis:alpine` on port 6379.
*   **rabbitmq**: `rabbitmq:4.0-management` on ports 5672 and 15672.

### 2. Custom Services
*   **user-api**: Depends on `mongodb` and `rabbitmq`.
*   **url-api**: Depends on `mongodb`, `redis`, and `rabbitmq`.
*   **api-gateway**: Depends on `user-api` and `url-api`.
*   **frontend**: Depends on `api-gateway`.

### 3. Networking
Docker Compose creates a default bridge network where containers can reach each other using their service names as hostnames.

## Implementation Plan

### Phase 1: Gateway Configuration for Docker
1.  Create `ApiGateway/ocelot.Docker.json` with hostnames instead of `localhost`.

### Phase 2: Docker Compose Definition
1.  Create `docker-compose.yml` in the root directory.
2.  Define all services and infrastructure.
3.  Configure environment variable overrides for connection strings.

## Examples

### ✅ Environment Variable Override in Compose
```yaml
user-api:
  environment:
    - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
    - RabbitMq__Host=rabbitmq
```

## Trade-offs
*   **PostgreSQL vs MongoDB**: The prompt mentioned PostgreSQL, but the project was recently migrated to MongoDB. I will use MongoDB to maintain consistency with the current codebase.

## Implementation Results
*   **ApiGateway/ocelot.Docker.json**: Added Docker-specific routing using service names and internal container ports (8080).
*   **ApiGateway/Program.cs**: Updated to load `ocelot.Docker.json` when `ASPNETCORE_ENVIRONMENT=Docker`.
*   **docker-compose.yml**: Created a complete orchestration file including `mongodb`, `redis`, `rabbitmq`, and the 4 custom services.
*   **Environment Overrides**: Used .NET environment variable syntax (`__`) to dynamically configure connection strings at runtime.

## Deviations from Original Design
*   **Infrastructure Selection**: Used MongoDB as the primary database instead of PostgreSQL to align with the previous migration work.
