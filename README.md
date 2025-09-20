# CRM Backend System

Бэкенд система для CRM (Customer Relationship Management) приложения на ASP.NET Core.

## Технологии

- .NET 8.0 - платформа разработки
- ASP.NET Core - веб-фреймворк
- Dapper - микро-ORM для работы с базой данных
- PostgreSQL - база данных (Npgsql)
- Swagger/OpenAPI - документация API
- Docker - контейнеризация

## Установка и запуск

### Предварительные требования
- .NET SDK 8.0 или выше
- PostgreSQL 12+
- Docker и Docker Compose (опционально)

### Установка

1. Клонируйте репозиторий:
```
git clone https://github.com/BulatRuslanovich/crm_back.git
cd crm_back
```

3. Восстановите зависимости:
``` dotnet restore ```

4. Настройте базу данных:
- Создайте базу данных PostgreSQL
- Выполните SQL скрипт из db_script.sql для создания таблиц
- Настройте подключение в appsettings.json

4. Запустите приложение:
# dev режим
``` dotnet run ```

# с hot reload
``` dotnet watch run ```

### Запуск с Docker

1. Запустите контейнеры:

``` docker-compose up -d ```

3. Приложение будет доступно по адресу: http://localhost:5000
4. Swagger UI: http://localhost:5000/swagger

## Конфигурация

Настройте подключение к базе данных в appsettings.json:

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

## Структура проекта

```
CrmBack/
├── Api/Controllers/          # Контроллеры API
│   └── UserController.cs
├── Core/                    # Ядро приложения
│   ├── Models/Entities/     # Сущности базы данных
│   │   ├── UserEntity.cs
│   │   ├── OrgEntity.cs
│   │   ├── ActivEntity.cs
│   │   ├── PlanEntity.cs
│   │   ├── PolicyEntity.cs
│   │   └── StatusEntity.cs
│   ├── Models/Payload/      # DTO модели
│   │   └── User/
│   │       ├── CreateUserPayload.cs
│   │       ├── ReadUserPayload.cs
│   │       └── UpdateUserPayload.cs
│   ├── Repositories/        # Интерфейсы репозиториев (IUserRepository.cs)
│   ├── Services/           # Интерфейсы сервисов (IUserService.cs)
│   └── Utils/Mapper/       # Мапперы (UserMapper.cs)
├── Data/Repositories/       # Реализации репозиториев (UserRepository.cs)
├── Services/               # Реализации сервисов (UserService.cs)
├── Properties/             
├── appsettings.json        # Конфигурация
├── Program.cs              # Точка входа
├── docker-compose.yaml     # Docker конфигурация
├── db_script.sql          # SQL скрипты для создания БД
└── CrmBack.http           # HTTP тесты
```

## API Endpoints

### Пользователи
- GET /api/users - Получить всех пользователей
- GET /api/users/{id} - Получить пользователя по ID
- POST /api/users - Создать пользователя
- PUT /api/users/{id} - Обновить пользователя
- DELETE /api/users/{id} - Удалить пользователя

API документация доступна через Swagger UI: /swagger

## Docker

Проект включает docker-compose.yaml для развертывания:

# Запуск приложения и PostgreSQL

``` docker-compose up -d ```

# Остановка

``` docker-compose down ```

# Просмотр логов

``` docker-compose logs ```

## Разработка

### Сборка проекта
``` dotnet build ```

### Работа с базой данных

1. Подключитесь к PostgreSQL
2. Выполните скрипт из db_script.sql для создания необходимых таблиц
3. При изменении структуры БД обновите соответствующие сущности в папке Core/Models/Entities/

### Пример запроса через CrmBack.http
```
GET https://localhost:7000/api/users
Accept: application/json
```

## Contributing

1. Форкните репозиторий
2. Создайте feature ветку: git checkout -b feature/amazing-feature
3. Закоммитьте изменения: git commit -m 'Add amazing feature'
4. Запушьте ветку: git push origin feature/amazing-feature
5. Откройте Pull Request
