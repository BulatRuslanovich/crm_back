using System.Data;
using CrmBack.Core.Models.Entities;
using CrmBack.Repository.Impl;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Tests.Integration;

public abstract class BasePostgreSqlIntegrationTest : IAsyncLifetime
{
    protected IDbConnection DbConnection { get; private set; } = null!;
    protected IDistributedCache Cache { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected UserRepository UserRepository { get; private set; } = null!;
    protected ActivRepository ActivRepository { get; private set; } = null!;
    protected OrgRepository OrgRepository { get; private set; } = null!;
    protected PlanRepository PlanRepository { get; private set; } = null!;

    private readonly PostgreSqlContainer _postgreSqlContainer;

    protected BasePostgreSqlIntegrationTest()
    {
        // Создаем PostgreSQL контейнер
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(5432, true)
            .Build();

        // Настраиваем Dapper для работы с snake_case
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Создаем кэш
        Cache = new MemoryDistributedCache(Options.Create(new Microsoft.Extensions.Caching.Memory.MemoryDistributedCacheOptions()));
    }

    public async Task InitializeAsync()
    {
        // Запускаем контейнер
        await _postgreSqlContainer.StartAsync();

        // Создаем подключение к PostgreSQL
        DbConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());

        // Открываем подключение
        DbConnection.Open();

        // Создаем репозитории
        UserRepository = new UserRepository(DbConnection, Cache);
        ActivRepository = new ActivRepository(DbConnection, Cache);
        OrgRepository = new OrgRepository(DbConnection, Cache);
        PlanRepository = new PlanRepository(DbConnection, Cache);

        // Настраиваем DI контейнер
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDbConnection>(_ => DbConnection);
        services.AddScoped<IDistributedCache>(_ => Cache);
        services.AddScoped<UserRepository>();
        services.AddScoped<ActivRepository>();
        services.AddScoped<OrgRepository>();
        services.AddScoped<PlanRepository>();

        ServiceProvider = services.BuildServiceProvider();

        // Создаем схему базы данных
        await CreateDatabaseSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        DbConnection?.Dispose();
        if (Cache is IDisposable disposableCache)
        {
            disposableCache.Dispose();
        }
        if (ServiceProvider is IDisposable disposableServiceProvider)
        {
            disposableServiceProvider.Dispose();
        }
        await _postgreSqlContainer.DisposeAsync();
    }

    private async Task CreateDatabaseSchemaAsync()
    {
        // Создаем таблицы для тестирования
        var createTablesSql = @"
            CREATE TABLE IF NOT EXISTS usr (
                usr_id SERIAL PRIMARY KEY,
                first_name VARCHAR(255) NOT NULL,
                middle_name VARCHAR(255),
                last_name VARCHAR(255) NOT NULL,
                login VARCHAR(255) UNIQUE NOT NULL,
                password_hash VARCHAR(255) NOT NULL,
                is_deleted BOOLEAN NOT NULL DEFAULT FALSE
            );

            CREATE TABLE IF NOT EXISTS org (
                org_id SERIAL PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                inn VARCHAR(255) UNIQUE NOT NULL,
                latitude REAL NOT NULL,
                longitude REAL NOT NULL,
                address TEXT NOT NULL,
                is_deleted BOOLEAN NOT NULL DEFAULT FALSE
            );

            CREATE TABLE IF NOT EXISTS status (
                status_id SERIAL PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                is_deleted BOOLEAN NOT NULL DEFAULT FALSE
            );

            CREATE TABLE IF NOT EXISTS activ (
                activ_id SERIAL PRIMARY KEY,
                usr_id INTEGER NOT NULL,
                org_id INTEGER NOT NULL,
                status_id INTEGER NOT NULL,
                visit_date DATE NOT NULL,
                start_time TIME NOT NULL,
                end_time TIME NOT NULL,
                description TEXT,
                is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
                FOREIGN KEY (usr_id) REFERENCES usr(usr_id),
                FOREIGN KEY (org_id) REFERENCES org(org_id),
                FOREIGN KEY (status_id) REFERENCES status(status_id)
            );

            CREATE TABLE IF NOT EXISTS plan (
                plan_id SERIAL PRIMARY KEY,
                usr_id INTEGER NOT NULL,
                org_id INTEGER NOT NULL,
                start_date DATE NOT NULL,
                end_date DATE NOT NULL,
                is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
                FOREIGN KEY (usr_id) REFERENCES usr(usr_id),
                FOREIGN KEY (org_id) REFERENCES org(org_id)
            );
        ";

        await DbConnection.ExecuteAsync(createTablesSql);
    }

    protected async Task<UserEntity> CreateTestUserAsync(string login = "testuser", string firstName = "Test", string lastName = "User")
    {
        var user = new UserEntity(0, firstName, "Middle", lastName, login, "hashed_password", false);
        var userId = await UserRepository.CreateAsync(user);

        if (userId == 0)
        {
            throw new InvalidOperationException($"CreateAsync returned invalid ID: {userId}");
        }

        return user with { usr_id = userId };
    }

    protected async Task<OrgEntity> CreateTestOrganizationAsync(string name = "Test Org", string inn = "1234567890")
    {
        var org = new OrgEntity(0, name, inn, 55.7558, 37.6176, "Test Address", false);
        var orgId = await OrgRepository.CreateAsync(org);
        return org with { org_id = orgId };
    }

    protected async Task<StatusEntity> CreateTestStatusAsync(string name = "Active")
    {
        var status = new StatusEntity(0, name, false);
        var statusId = await DbConnection.ExecuteScalarAsync<int>(
            "INSERT INTO status (name, is_deleted) VALUES (@name, @is_deleted) RETURNING status_id;",
            new { name = status.name, is_deleted = status.is_deleted });
        return status with { status_id = statusId };
    }

    protected async Task<ActivEntity> CreateTestActivityAsync(int userId, int orgId, int statusId)
    {
        var activity = new ActivEntity(0, userId, orgId, statusId, DateTime.Today, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Test Activity", false);
        var activityId = await ActivRepository.CreateAsync(activity);
        return activity with { activ_id = activityId };
    }

    protected async Task<PlanEntity> CreateTestPlanAsync(int userId, int orgId)
    {
        var plan = new PlanEntity(0, userId, orgId, DateTime.Today, DateTime.Today.AddDays(1), false);
        var planId = await PlanRepository.CreateAsync(plan);
        return plan with { plan_id = planId };
    }

    protected async Task CleanupDatabaseAsync()
    {
        await DbConnection.ExecuteAsync("DELETE FROM plan");
        await DbConnection.ExecuteAsync("DELETE FROM activ");
        await DbConnection.ExecuteAsync("DELETE FROM status");
        await DbConnection.ExecuteAsync("DELETE FROM org");
        await DbConnection.ExecuteAsync("DELETE FROM usr");
    }
}

// Type handlers для Dapper
public class Int32Handler : SqlMapper.TypeHandler<int>
{
    public override void SetValue(IDbDataParameter parameter, int value)
    {
        parameter.Value = value;
    }

    public override int Parse(object value)
    {
        return Convert.ToInt32(value);
    }
}

public class Int64Handler : SqlMapper.TypeHandler<long>
{
    public override void SetValue(IDbDataParameter parameter, long value)
    {
        parameter.Value = value;
    }

    public override long Parse(object value)
    {
        return Convert.ToInt64(value);
    }
}

public class BooleanHandler : SqlMapper.TypeHandler<bool>
{
    public override void SetValue(IDbDataParameter parameter, bool value)
    {
        parameter.Value = value;
    }

    public override bool Parse(object value)
    {
        return Convert.ToBoolean(value);
    }
}

public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value;
    }

    public override DateTime Parse(object value)
    {
        return Convert.ToDateTime(value);
    }
}

public class TimeSpanHandler : SqlMapper.TypeHandler<TimeSpan>
{
    public override void SetValue(IDbDataParameter parameter, TimeSpan value)
    {
        parameter.Value = value;
    }

    public override TimeSpan Parse(object value)
    {
        return TimeSpan.Parse(value.ToString()!);
    }
}
