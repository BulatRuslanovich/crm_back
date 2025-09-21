using System.Data;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Data.Repositories;
using CrmBack.Services;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(static option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Crm API",
        Version = "v1",
        Description = "CRM API"
    });
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// connect to db
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DbConnectionString");
    
    return new NpgsqlConnection(connectionString);
});



builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActivRepository, ActivRepository>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var connection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
        connection.Open();
        Console.WriteLine("Database connection successful");
        connection.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();

