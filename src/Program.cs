using System.Data;
using System.Text;
using CrmBack.Core.Config;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Data.Repositories;
using CrmBack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {LogType:l} {Message:lj}{NewLine}{Exception}"
    );

    config.WriteTo.Debug();
    config.MinimumLevel.Warning();
    config.MinimumLevel.Override("CrmBack.Data", Serilog.Events.LogEventLevel.Debug);
    config.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Information);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.WithOrigins("http://localhost:4040", "https://localhost:4041")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Crm API",
        Version = "v1",
        Description = "CRM API"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    option.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT-token in format: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))
        ),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DbConnectionString");
    return new NpgsqlConnection(connectionString);
});

builder.Services.Configure<DatabaseLoggingOptions>(builder.Configuration.GetSection("DatabaseLogging"));


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActivRepository, ActivRepository>();
builder.Services.AddScoped<IOrgRepository, OrgRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActivService, ActivService>();
builder.Services.AddScoped<IOrgService, OrgService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var errorFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (errorFeature != null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
            Log.Error(errorFeature.Error, "Unhandled exception in {Path}", context.Request.Path);
        }
    });
});


app.UseSerilogRequestLogging();
app.UseCors("AllowSwagger");

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5555");
