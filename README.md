<img src="https://readme-typing-svg.herokuapp.com?font=Lexend+Giga&size=25&pause=1000&color=6495ED&vCenter=true&width=435&height=25&lines=CRM%20Backend%20System" width="450"/>

---

<p align="center">
  <img src="https://img.shields.io/github/actions/workflow/status/BulatRuslanovich/crm_back/build.yaml?branch=master&style=for-the-badge&label=Build" alt="Build" />
  <img src="https://img.shields.io/codecov/c/github/BulatRuslanovich/crm_back?style=for-the-badge&label=Coverage" alt="Coverage" />
  <img src="https://img.shields.io/github/last-commit/BulatRuslanovich/crm_back/master?label=Last%20Commit&color=blue&style=for-the-badge" alt="Last Commit" />
  <img src="https://img.shields.io/github/repo-size/BulatRuslanovich/crm_back?label=Repo%20Size&color=orange&style=for-the-badge" alt="Repo Size" />
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge" alt=".NET Version" />
  <img src="https://img.shields.io/badge/PostgreSQL-16%2B-blue?style=for-the-badge" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Status-In%20Development-orange?style=for-the-badge" alt="Status" />
  <img src="https://img.shields.io/github/license/BulatRuslanovich/crm_back?color=yellow&style=for-the-badge" alt="License" />
</p>

Backend API for CRM system built with ASP.NET Core 8, PostgreSQL, and Redis.

> **⚠️ Project Status**: In active development. API endpoints and structure may change.

---

## Tech Stack

- **ASP.NET Core 8** - Modern web framework
- **PostgreSQL 16+** - Relational database
- **Redis** - Caching and session management
- **Dapper** - High-performance ORM
- **JWT Authentication** - Secure token-based auth
- **xUnit** - Unit and integration testing

---

## Core Features

- **Authentication System** - JWT-based user authentication
- **CRUD Operations** - Generic repository pattern for all entities
- **Data Validation** - Input validation and sanitization
- **Caching** - Redis integration for performance
- **API Documentation** - Swagger/OpenAPI integration
- **Comprehensive Testing** - Unit and integration test coverage

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 16+
- Redis 7+
- Docker & Docker Compose (optional, for services)

### Quick Start

```bash
# Run application directly
dotnet run

# Or via Docker Compose with all services
docker-compose up -d
```

### Database Setup

```bash
# Initialize databases (PostgreSQL + Redis)
docker-compose up -d postgres redis

# Or all services including Nginx
docker-compose up -d
```

### API Documentation

Access Swagger UI at: `http://localhost/swagger` (via Nginx)
or directly: `http://localhost:5555/swagger`

---

## Architecture

### Why Nginx?

Nginx serves as a **reverse proxy** in front of ASP.NET Core, providing:

1. **Rate Limiting**: 100 req/s per IP for API, 10 req/s for Swagger (DDoS protection)
2. **Connection Limits**: Max 20 connections per IP
3. **Load Balancing**: Ready for horizontal scaling with multiple ASP.NET instances
4. **Security Headers**: X-Frame-Options, X-Content-Type-Options, etc.
5. **Caching Layer**: Additional caching on top of Redis/Response Cache
6. **Keep-Alive Connections**: Reuses connections to backend
7. **Centralized Logging**: Single point for access/error logs
8. **SSL Termination**: Ready for HTTPS (with certbot)

**In production**: Single entry point (Port 80) → Nginx → ASP.NET Core (Port 5555)

---

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Formatting

```bash
./format-code.sh
```


### Project Structure

```
src/
├── Controllers/     # API endpoints
├── Core/           # Business logic & models
├── Repository/     # Data access layer
├── Services/       # Business services
└── Program.cs      # Application entry point
```




