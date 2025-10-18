<img src="https://readme-typing-svg.herokuapp.com?font=Lexend+Giga&size=25&pause=1000&color=6495ED&vCenter=true&width=435&height=25&lines=CRM%20Backend%20System" width="450"/>

---

<p align="center">
  <img src="https://img.shields.io/github/actions/workflow/status/BulatRuslanovich/crm_back/build.yaml?branch=master&style=for-the-badge&label=Build" alt="Build" />
  <img src="https://img.shields.io/codecov/c/github/BulatRuslanovich/crm_back?style=for-the-badge&label=Coverage" alt="Coverage" />
  <img src="https://img.shields.io/github/last-commit/BulatRuslanovich/crm_back/master?label=Last%20Commit&color=blue&style=for-the-badge" alt="Last Commit" />
  <img src="https://img.shields.io/github/repo-size/BulatRuslanovich/crm_back?label=Repo%20Size&color=orange&style=for-the-badge" alt="Repo Size" />
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge" alt=".NET Version" />
  <img src="https://img.shields.io/badge/PostgreSQL-16%2B-blue?style=for-the-badge" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Redis-7%2B-red?style=for-the-badge" alt="Redis" />
  <img src="https://img.shields.io/badge/Status-In%20Development-orange?style=for-the-badge" alt="Status" />
  <img src="https://img.shields.io/github/license/BulatRuslanovich/crm_back?color=yellow&style=for-the-badge" alt="License" />
</p>


A robust backend system for a Customer Relationship Management (CRM) application, built with ASP.NET Core 8, PostgreSQL, and Redis.

> **⚠️ Project Status**: Currently in active development. API endpoints and structure may change.

## Tech Stack

- **ASP.NET Core 8** - Modern web framework
- **PostgreSQL 16+** - Reliable data storage
- **Redis** - Caching and session management
- **Dapper** - Micro ORM for efficient data access
- **JWT** - Authentication and authorization
- **Swagger/OpenAPI** - API documentation
- **Serilog** - Structured logging
- **Docker Compose** - Container orchestration

## Architecture

```
src/
├── Api/Controllers/     # REST API endpoints
├── Core/               # Business logic interfaces
├── Data/Repositories/  # Data access layer
├── Services/           # Business logic implementation
└── Models/             # Data models and DTOs
```

## API Overview

### Authentication
- `POST /api/user/login` - User authentication
- `POST /api/user/register` - User registration

### Core Entities
- **Users** - System users management
- **Organizations** - Client organizations
- **Activities** - Customer interactions
- **Plans** - Work planning and scheduling


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.