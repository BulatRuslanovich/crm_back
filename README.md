# CRM Backend System

A robust backend system for a Customer Relationship Management (CRM) application, built with ASP.NET Core for scalable API development.

## Overview

This project provides a RESTful API backend for managing customer relationships, including endpoints for customers, interactions, and analytics. It leverages modern .NET practices for performance and maintainability.

## Technologies

| Technology       | Purpose                          | Version/Notes          |
|------------------|----------------------------------|------------------------|
| .NET             | Development platform             | 8.0+                   |
| ASP.NET Core     | Web framework                    | -                      |
| Dapper           | Micro-ORM for database operations| -                      |
| PostgreSQL       | Relational database              | 12+ (via Npgsql)       |
| Swagger/OpenAPI  | API documentation                | -                      |
| Docker           | Containerization                 | -                      |

## Prerequisites

Before setting up the project, ensure you have the following installed:

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/products/docker-desktop) and [Docker Compose](https://docs.docker.com/compose/install/) (optional, for containerized deployment)
- Git

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/BulatRuslanovich/crm_back.git
cd crm_back
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Set Up the Database

1. Create a new PostgreSQL database (e.g., `crm_db`).
2. Execute the schema initialization script:

   ```bash
   psql -U postgres -d crm_db -f db_script.sql
   ```

3. Update the connection string in `appsettings.json` (see [Configuration](#configuration) below).

### 4. Run the Application

#### Development Mode

```bash
dotnet run
```

#### With Hot Reload (for development)

```bash
dotnet watch run
```

The API will be available at `http://localhost:5000`. Access the interactive Swagger UI at `http://localhost:5000/swagger`.

## Running with Docker

For a containerized setup, use Docker Compose to spin up both the application and PostgreSQL database.

### 1. Build and Start Containers

```bash
docker-compose up -d --build
```

### 2. Access the Application

- API Base URL: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- Database (if needed): `localhost:5432`

### 3. Stop Containers

```bash
docker-compose down
```

To remove volumes (e.g., reset database):

```bash
docker-compose down -v
```

## Configuration

The application configuration is managed via `appsettings.json`. Key sections include:

### Database Connection

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crm_db;Username=postgres;Password=your_password;"
  }
}
```

### Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

For production environments, create an `appsettings.Production.json` file and override settings as needed. Environment variables can also be used for sensitive data like passwords.

## API Endpoints

The API provides the following core endpoints (documented in Swagger):

- `GET /api/customers` - Retrieve all customers
- `POST /api/customers` - Create a new customer
- `PUT /api/customers/{id}` - Update a customer
- `DELETE /api/customers/{id}` - Delete a customer
- Additional endpoints for interactions, reports, etc.

For full details, explore the Swagger UI after starting the application.

## Project Structure

```
crm_back/
├── Controllers/          # API controllers
├── Data/                 # Database context and repositories
├── Models/               # Domain models and DTOs
├── Services/             # Business logic services
├── db_script.sql        # Database schema script
├── Dockerfile           # Docker configuration
├── docker-compose.yml   # Container orchestration
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration file
```

## Contributing

We welcome contributions! To get started:

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/amazing-feature`.
3. Make your changes and commit: `git commit -m "Add amazing feature"`.
4. Push to the branch: `git push origin feature/amazing-feature`.
5. Open a Pull Request on GitHub.

Please ensure your code follows these guidelines:
- Write unit tests for new features.
- Update documentation if necessary.
- Keep commits atomic and descriptive.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter issues, please open an issue on GitHub with details about your environment and the problem. For questions, feel free to reach out via the repository's Discussions tab.

