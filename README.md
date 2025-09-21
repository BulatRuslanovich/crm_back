# CRM Backend System

A backend system for a CRM (Customer Relationship Management) application built on ASP.NET Core.

## Technologies

- .NET 8.0 - Development platform
- ASP.NET Core - Web framework
- Dapper - Micro-ORM for database operations
- PostgreSQL - Database (using Npgsql)
- Swagger/OpenAPI - API documentation
- Docker - Containerization

## Installation and Running

### Prerequisites
- .NET SDK 8.0 or higher
- PostgreSQL 12+
- Docker and Docker Compose (optional)

### Installation

1. Clone the repository:
```
git clone https://github.com/BulatRuslanovich/crm_back.git
cd crm_back
```

3. Restore dependencies:
``` dotnet restore ```

4. Set up the database:
- Create a PostgreSQL database
- Execute the SQL script from ```db_script.sql``` to create the tables
- Configure the connection string in ```appsettings.json```

4. Run the application:
# Development mode
``` dotnet run ```

# With hot reload
``` dotnet watch run ```

### Running with Docker

1. Start the containers:

``` docker-compose up -d ```

3. The application will be available at: ```http://localhost:5000```
4. Swagger UI: ```http://localhost:5000/swagger```

## Configuration

Configure the database connection in ```appsettings.json```:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crm_db;Username=postgres;Password=your_password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Contributing

1. Fork the repository
2. Create a feature branch: ```git checkout -b feature/amazing-feature```
3. Commit your changes: ```git commit -m 'Add amazing feature```
4. Push the branch: ```git push origin feature/amazing-feature```
5. Open a Pull Request
