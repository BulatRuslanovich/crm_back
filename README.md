# CRM Backend System

<p align="center">
  <img src="https://github.com/BulatRuslanovich/crm_back/actions/workflows/build.yaml/badge.svg" alt="Build" />
  <img src="https://codecov.io/gh/BulatRuslanovich/crm_back/branch/master/graph/badge.svg?token=YOUR_CODECOV_TOKEN" alt="Coverage" />
  <img src="https://img.shields.io/github/last-commit/BulatRuslanovich/crm_back/master?label=Last%20Commit&color=blue" alt="Last Commit" />
  <img src="https://img.shields.io/github/repo-size/BulatRuslanovich/crm_back?label=Repo%20Size&color=orange" alt="Repo Size" />
  <img src="https://img.shields.io/badge/.NET-8.0-purple" alt=".NET Version" />
  <img src="https://img.shields.io/badge/PostgreSQL-12%2B-blue" alt="PostgreSQL" />
  <img src="https://img.shields.io/github/license/BulatRuslanovich/crm_back?color=yellow" alt="License" />
</p>


A robust backend system for a Customer Relationship Management (CRM) application, built with ASP.NET Core for scalable API development.

## Overview

This project provides a RESTful API backend for managing customer relationships, including endpoints for customers, interactions, and analytics. It leverages modern .NET practices for performance and maintainability.

## Tech Stack

| Technology | Role | Version |
|-------------|------|----------|
| [.NET 8.0](https://dotnet.microsoft.com/) | Backend framework | LTS |
| [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core) | Web API layer | — |
| [Dapper](https://github.com/DapperLib/Dapper) | Lightweight ORM | — |
| [PostgreSQL](https://www.postgresql.org/) | Relational database | 12+ |
| [Swagger / OpenAPI](https://swagger.io/tools/open-source/open-source-integrations/) | API documentation | — |
| [Docker](https://www.docker.com/) | Containerization | — |
| [GitHub Actions](https://github.com/features/actions) | CI/CD automation | — |
| [Codecov](https://about.codecov.io/) | Test coverage reports | — |

[Coverage](https://app.codecov.io/github/bulatruslanovich/crm_back)

---

##  API Endpoints

The CRM API provides endpoints for managing users, organizations, and activities.
All routes are versioned under `/api/*` and protected where necessary by authorization.

---

### User API (`/api/user`)

| Method   | Endpoint          | Auth | Description                      |
| :------- | :---------------- | :--: | :------------------------------- |
| `GET`    | `/api/user/{id}`  |   ✅  | Retrieve a user by ID            |
| `GET`    | `/api/user`       |   ✅  | Retrieve all users               |
| `POST`   | `/api/user`       |   ❌  | Create a new user                |
| `PUT`    | `/api/user/{id}`  |   ✅  | Update an existing user          |
| `DELETE` | `/api/user/{id}`  |   ✅  | Delete a user by ID              |
| `POST`   | `/api/user/login` |   ❌  | Authenticate user and return JWT |

---

### Organization API (`/api/org`)

| Method   | Endpoint        | Auth | Description                    |
| :------- | :-------------- | :--: | :----------------------------- |
| `GET`    | `/api/org/{id}` |   ✅  | Retrieve an organization by ID |
| `GET`    | `/api/org`      |   ✅  | Retrieve all organizations     |
| `POST`   | `/api/org`      |   ✅  | Create a new organization      |
| `PUT`    | `/api/org/{id}` |   ✅  | Update an organization         |
| `DELETE` | `/api/org/{id}` |   ✅  | Delete an organization by ID   |

---

### Activity API (`/api/activ`)

| Method   | Endpoint          | Auth | Description                |
| :------- | :---------------- | :--: | :------------------------- |
| `GET`    | `/api/activ/{id}` |   ✅  | Retrieve an activity by ID |
| `GET`    | `/api/activ`      |   ✅  | Retrieve all activities    |
| `POST`   | `/api/activ`      |   ✅  | Create a new activity      |
| `PUT`    | `/api/activ/{id}` |   ✅  | Update an activity         |
| `DELETE` | `/api/activ/{id}` |   ✅  | Delete an activity by ID   |

---

### Authentication

* All protected endpoints require a valid JWT token in the `Authorization` header:

  ```
  Authorization: Bearer <your_token_here>
  ```

---



## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

