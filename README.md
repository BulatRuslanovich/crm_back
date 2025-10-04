<img src="https://readme-typing-svg.herokuapp.com?font=Lexend+Giga&size=25&pause=1000&color=6495ED&vCenter=true&width=435&height=25&lines=CRM%20Backend%20System" width="450"/>

---

<p align="center">
  <img src="https://img.shields.io/github/actions/workflow/status/BulatRuslanovich/crm_back/build.yaml?branch=master&style=for-the-badge&label=Build" alt="Build" />
  <img src="https://img.shields.io/codecov/c/github/BulatRuslanovich/crm_back?style=for-the-badge&label=Coverage" alt="Coverage" />
  <img src="https://img.shields.io/github/last-commit/BulatRuslanovich/crm_back/master?label=Last%20Commit&color=blue&style=for-the-badge" alt="Last Commit" />
  <img src="https://img.shields.io/github/repo-size/BulatRuslanovich/crm_back?label=Repo%20Size&color=orange&style=for-the-badge" alt="Repo Size" />
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge" alt=".NET Version" />
  <img src="https://img.shields.io/badge/PostgreSQL-12%2B-blue?style=for-the-badge" alt="PostgreSQL" />
  <img src="https://img.shields.io/github/license/BulatRuslanovich/crm_back?color=yellow&style=for-the-badge" alt="License" />
</p>


A robust backend system for a Customer Relationship Management (CRM) application, built with ASP.NET Core for scalable API development.

<img src="https://readme-typing-svg.herokuapp.com?font=Lexend+Giga&size=25&pause=1000&color=6495ED&vCenter=true&width=435&height=25&lines=Overview" width="450"/>

---

This project provides a RESTful API backend for managing customer relationships, including endpoints for customers, interactions, and analytics. It leverages modern .NET practices for performance and maintainability.

<img src="https://readme-typing-svg.herokuapp.com?font=Lexend+Giga&size=25&pause=1000&color=6495ED&vCenter=true&width=435&height=25&lines=Tech%20Stack" width="450"/>

---

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


<img src="https://readme-typing-svg.herokuapp.com?font=Lexend+Giga&size=25&pause=1000&color=6495ED&vCenter=true&width=435&height=25&lines=API%20Endpoints" width="450"/>

---

The CRM API provides endpoints for managing users, organizations, and activities.
All routes are versioned under `/api/*` and protected where necessary by authorization.



### User API (`/api/user`)

| Method   | Endpoint          | Auth | Description                      |
| :------- | :---------------- | :--: | :------------------------------- |
| `GET`    | `/api/user/{id}`  |   ✅  | Retrieve a user by ID            |
| `GET`    | `/api/user`       |   ✅  | Retrieve all users               |
| `POST`   | `/api/user`       |   ❌  | Create a new user                |
| `PUT`    | `/api/user/{id}`  |   ✅  | Update an existing user          |
| `DELETE` | `/api/user/{id}`  |   ✅  | Delete a user by ID              |
| `POST`   | `/api/user/login` |   ❌  | Authenticate user and return JWT |


### Organization API (`/api/org`)

| Method   | Endpoint        | Auth | Description                    |
| :------- | :-------------- | :--: | :----------------------------- |
| `GET`    | `/api/org/{id}` |   ✅  | Retrieve an organization by ID |
| `GET`    | `/api/org`      |   ✅  | Retrieve all organizations     |
| `POST`   | `/api/org`      |   ✅  | Create a new organization      |
| `PUT`    | `/api/org/{id}` |   ✅  | Update an organization         |
| `DELETE` | `/api/org/{id}` |   ✅  | Delete an organization by ID   |


### Activity API (`/api/activ`)

| Method   | Endpoint          | Auth | Description                |
| :------- | :---------------- | :--: | :------------------------- |
| `GET`    | `/api/activ/{id}` |   ✅  | Retrieve an activity by ID |
| `GET`    | `/api/activ`      |   ✅  | Retrieve all activities    |
| `POST`   | `/api/activ`      |   ✅  | Create a new activity      |
| `PUT`    | `/api/activ/{id}` |   ✅  | Update an activity         |
| `DELETE` | `/api/activ/{id}` |   ✅  | Delete an activity by ID   |



## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

