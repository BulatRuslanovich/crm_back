# CRM Backend API

High-performance REST API for CRM system. ASP.NET Core 8, PostgreSQL, Redis.

## Tech Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Framework | ASP.NET Core 8 | Web API |
| Database | PostgreSQL 16 | Primary storage |
| Cache | Redis 7 + MessagePack + LZ4 | Distributed caching |
| Proxy | Nginx | Rate limiting, load balancing |
| Auth | JWT + BCrypt | Token-based auth |
| Validation | FluentValidation | Input validation |
| Logging | Serilog | Structured logging |

## Architecture

```
nginx:80 → ASP.NET Core:5555 → PostgreSQL:5432
                        ↓
                   Redis:6379
```

**Layers**: Controllers → Services → DAOs → Database

**Patterns**:
- Generic CRUD controller (`CrudController<T>`)
- Repository pattern with caching
- Soft delete with `BaseEntity.IsDeleted`
- DTO → Entity mapping

## Entities

| Entity | Description | CRUD |
|--------|-------------|------|
| User | Users with roles (Admin, Director, Manager, Representative) | ✅ |
| Org | Organizations (name, INN, GPS, address) | ✅ |
| Activ | User activities (visits to orgs with status) | ✅ |
| Policy | Role permissions | System |
| Status | Activity statuses | System |

**Relations**: User → Policy (many-to-many), User → Activ (one-to-many), Org → Activ (one-to-many)

## Quick Start

```bash
# Prerequisites
.NET 8 SDK, PostgreSQL 16+, Redis 7+

# Run services
docker-compose up -d postgres redis nginx

# Run app
dotnet run

# Access API
http://localhost:5555/swagger
```

**Default user**: `bulat` / `1234` (Admin)

## API Endpoints

```
POST   /api/v1/auth/login        # Login (returns JWT in cookies)
POST   /api/v1/auth/register     # Register user
POST   /api/v1/auth/refresh      # Refresh token
POST   /api/v1/auth/logout       # Logout (clears cookies)

GET    /api/v1/user              # List users (paginated)
GET    /api/v1/user/{id}         # Get user
POST   /api/v1/user              # Create user
PUT    /api/v1/user/{id}         # Update user
DELETE /api/v1/user/{id}         # Delete user (soft)
GET    /api/v1/user/{id}/activ   # Get user activities

GET    /api/v1/org               # List orgs (paginated)
GET    /api/v1/org/{id}          # Get org
POST   /api/v1/org               # Create org
PUT    /api/v1/org/{id}          # Update org
DELETE /api/v1/org/{id}          # Delete org (soft)

GET    /api/v1/activ             # List activities (paginated)
GET    /api/v1/activ/{id}        # Get activity
POST   /api/v1/activ             # Create activity
PUT    /api/v1/activ/{id}        # Update activity
DELETE /api/v1/activ/{id}        # Delete activity (soft)
```

## Project Structure

```
├── Controllers/       # API endpoints (CrudController base)
├── Services/          # Business logic (IService<T>)
├── DAOs/              # Data access (ICrudDAO<T>, BaseCrudDAO)
├── Core/
│   ├── Models/
│   │   ├── Dto/       # Data Transfer Objects (records)
│   │   └── Entities/  # EF entities
│   ├── Utils/         # Helpers, compiled queries
│   ├── Validators/    # FluentValidation rules
│   └── Middleware/    # Exception handler, token refresh
├── Data/              # DbContext
└── Program.cs         # Bootstrap
```

## Performance

**Load test** (wrk -t4 -c100 -d10s):
- **Throughput**: 12,620 req/s
- **Latency**: 5.47ms avg (92% < 8ms)
- **Target**: `/api/v1/activ` with cache hit

**Optimizations**:
- MessagePack + LZ4 caching (5min TTL, 3x faster than JSON, 4x smaller)
- Pattern-based cache invalidation (Redis SCAN)
- Compiled queries for hot paths (~2x faster SQL)
- Connection pooling (PostgreSQL: 5-100, Redis tuned)
- Response caching (120s GET), compression (Gzip/Brotli)
- AsNoTracking for reads, production EF settings
- PostgreSQL: GIN indexes, partial indexes, trigram FTS
- FluentValidation-only (DataAnnotations suppressed)

## Security

- JWT in HttpOnly cookies
- BCrypt password hashing
- Refresh token rotation (single token per user)
- Soft delete (data recovery)
- Role-based authorization
- Nginx rate limiting (100 req/s)

