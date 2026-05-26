# Slottet

Slottet is a web-based care management platform for a residential care facility. It allows care staff to manage resident information, track moods and risk levels, record shift types, and administer medications. An audit log records every significant action for accountability and GDPR compliance.

The system consists of two independently deployable applications:

- **API** — ASP.NET Core Web API. Handles authentication, business logic, and all database operations.
- **SlottetBlazor** — ASP.NET Core Blazor Server. The frontend application. Communicates exclusively with the API over HTTP.

---

## Architecture

```
Browser
  |
  | HTTP (SignalR WebSocket)
  v
SlottetBlazor  (Blazor Server, port 5050)
  |
  | HTTP — server-to-server (internal network in Docker)
  v
API            (ASP.NET Core Web API, port 5000)
  |
  | TCP
  v
MariaDB / MySQL
```

Because the Blazor application runs as a Blazor Server app, all component logic executes on the server. API calls are therefore server-to-server and do not pass through the browser. CORS on the API is relevant only for direct browser-based tool access (e.g. Swagger) and future migration to Blazor WASM.

---

## Technology Stack

| Layer         | Technology                              |
|---------------|-----------------------------------------|
| Frontend      | ASP.NET Core Blazor Server (.NET 9)     |
| API           | ASP.NET Core Web API (.NET 9)           |
| Auth          | ASP.NET Core Identity + JWT Bearer     |
| ORM           | Entity Framework Core 9 + Pomelo MySQL |
| Database      | MariaDB / MySQL 8                       |
| Containerisation | Docker + Docker Compose              |

---

## Project Structure

```
Slottet/
  API/                  Web API — controllers, middleware, services
  SlottetBlazor/        Blazor Server frontend
  Application/          DTOs and application-layer interfaces
  Domain/               Entities and enums
  Infrastructure/       EF Core DbContext, migrations, seed data
  docker-compose.yml    Orchestrates API + Blazor as separate services
  .env                  Local secrets (git-ignored, never commit)
  .env.example          Template — copy to .env and fill in values
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerised deployment)
- A running MariaDB or MySQL 8 instance

---

## Local Development (without Docker)

### 1. Configure environment variables

Copy `.env.example` to `.env` and fill in your values:

```
cp .env.example .env
```

The API reads `.env` automatically on startup via `DotNetEnv`. The file is resolved by traversing upward from the working directory, so placing it at the solution root covers both projects.

### 2. Start the API

```
cd Slottet/API
dotnet run
```

The API starts on `http://localhost:5050` (see `launchSettings.json`).

### 3. Start Blazor

In a separate terminal:

```
cd Slottet/SlottetBlazor
dotnet run
```

Blazor starts on `http://localhost:5140` and reads `ApiBaseUrl` from `appsettings.json` (defaults to `http://localhost:5000`). Override for a different API address by setting the `ApiBaseUrl` environment variable before running.

### 4. Apply database migrations

Migrations run automatically on API startup. To run them manually:

```
cd Slottet/API
dotnet ef database update
```

---

## Docker Deployment (Distributed)

The Docker setup runs the API and Blazor as separate containers connected via an internal bridge network. The Blazor container calls the API using the internal container name `slottet-api`, never exposing internal traffic to the host.

### 1. Configure environment variables

```
cp .env.example .env
```

Fill in all required values. At minimum:

| Variable      | Description                        |
|---------------|------------------------------------|
| `DB_HOST`     | Database server hostname or IP     |
| `DB_PORT`     | Database port (default: 3306)      |
| `DB_NAME`     | Database name                      |
| `DB_USER`     | Database user                      |
| `DB_PASSWORD` | Database password                  |
| `JWT_KEY`     | JWT signing key, minimum 32 chars  |

### 2. Build and start

```
docker compose up --build -d
```

This command:
1. Builds the API image using `API/Dockerfile` (build context: solution root)
2. Builds the Blazor image using `SlottetBlazor/Dockerfile` (build context: solution root)
3. Starts both containers on the `slottet-net` bridge network
4. Blazor waits for the API health check to pass before starting (`depends_on: condition: service_healthy`)

### 3. Verify

- API health check: `http://localhost:5000/health`
- Blazor frontend:  `http://localhost:5050`

### 4. Stop

```
docker compose down
```

### Network topology in Docker

```
Host machine
  |-- port 5000 --> slottet-api:8080   (API container)
  |-- port 5050 --> slottet-blazor:8080 (Blazor container)

slottet-net (internal bridge network)
  slottet-blazor --> http://slottet-api:8080  (Blazor calls API internally)
```

The API port (`API_PORT`) and Blazor port (`BLAZOR_PORT`) can be overridden in `.env`.

---

## Environment Variables Reference

All variables are read by the API via `DotNetEnv` (`.env` file) or directly from the process environment. Docker Compose passes them from `.env` into the containers.

### API

| Variable           | Required | Default                      | Description                                   |
|--------------------|----------|------------------------------|-----------------------------------------------|
| `DB_HOST`          | Yes      | —                            | Database server host                          |
| `DB_PORT`          | No       | `3306`                       | Database server port                          |
| `DB_NAME`          | Yes      | —                            | Database name                                 |
| `DB_USER`          | Yes      | —                            | Database username                             |
| `DB_PASSWORD`      | Yes      | —                            | Database password                             |
| `Jwt__Key`         | Yes      | —                            | JWT signing key (min. 32 characters)          |
| `Jwt__Issuer`      | No       | `SlottetApi`                 | JWT issuer claim                              |
| `Jwt__Audience`    | No       | `SlottetBlazor`              | JWT audience claim                            |
| `BLAZOR_ORIGIN`    | No       | `http://localhost:5140,...`  | Comma-separated CORS-allowed origins          |

> Variable names using `__` (double underscore) map to nested configuration sections in .NET.
> `Jwt__Key` resolves to `builder.Configuration["Jwt:Key"]`.

### Blazor

| Variable      | Required | Default                  | Description                     |
|---------------|----------|--------------------------|---------------------------------|
| `ApiBaseUrl`  | No       | `http://localhost:5000`  | Base URL of the API             |

In Docker, `ApiBaseUrl` is set to `http://slottet-api:8080` (internal container name) by `docker-compose.yml`.

---

## Authentication

Login is performed via `POST /api/auth/login` with email and a numeric PIN code. A successful login returns a JWT token that must be included as a `Bearer` token in subsequent authenticated requests.

Roles:

| Role              | Access level                                          |
|-------------------|-------------------------------------------------------|
| `Admin`           | Full access including employee management            |
| `Vagtansvarlig`   | Schedule management, read all employees              |
| `Plejepersonale`  | Read/update residents, record shifts and statuses    |

First-time login redirects to PIN setup if no PIN has been configured yet.

---

## API Endpoints Overview

| Method | Path                            | Auth policy        | Description                            |
|--------|---------------------------------|--------------------|----------------------------------------|
| POST   | `/api/auth/login`               | None               | Login — returns JWT                    |
| POST   | `/api/auth/setup-pincode`       | None               | Set PIN for first-time users           |
| POST   | `/api/auth/assign-role`         | RequireAdmin       | Assign Identity role to employee       |
| GET    | `/api/employees`                | RequireScheduler   | List all employees                     |
| POST   | `/api/employees`                | RequireAdmin       | Create employee                        |
| PUT    | `/api/employees/{id}`           | RequireAdmin       | Update employee                        |
| DELETE | `/api/employees/{id}`           | RequireAdmin       | GDPR-delete employee                   |
| GET    | `/api/resident`                 | None               | List all residents (with statuses)     |
| GET    | `/api/resident/public`          | None               | Public board view (no personal data)   |
| PUT    | `/api/resident/{id}`            | RequireCareStaff   | Update resident, mood, risk, status    |
| POST   | `/api/shifts`                   | RequireCareStaff   | Create or update today's shift         |
| GET    | `/api/shifts/today`             | RequireCareStaff   | Get current employee's shift for today |
| GET    | `/health`                       | None               | Health check endpoint                  |

---

## GDPR and Audit Logging

Every data-modifying operation creates an entry in the `AuditLogs` table including the acting user's ID and name. Employee deletion is a hard delete that removes all associated data.

---

## Development Notes

- Running `dotnet run` from a project subfolder causes `DotNetEnv` to traverse upward and find `.env` at the solution root. This is intentional.
- The `.env` file is git-ignored. Never commit it. Use `.env.example` as the canonical reference for required variables.
- The `appsettings.json` files contain no secrets. They hold only safe structural defaults.
- In Docker, environment variables passed via `docker-compose.yml` take precedence over `appsettings.json` values through the standard .NET configuration priority chain.
- The `docker-compose.yml` health check polls `GET /health` every 30 seconds. The Blazor container will not start until the API is healthy.
