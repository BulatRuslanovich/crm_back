using System;
using System.Threading.Tasks;
using CrmBack.Application.Activities.Services;
using CrmBack.Application.Auth.Services;
using CrmBack.Application.Auth.Validators;
using CrmBack.Application.Common.Validators;
using CrmBack.Application.Organizations.Services;
using CrmBack.Application.Users.Services;
using CrmBack.Application.Users.Validators;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Activities;
using CrmBack.Infrastructure.Persistence.Auth;
using CrmBack.Infrastructure.Persistence.Organizations;
using CrmBack.Infrastructure.Persistence.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, config) =>
{
	config
		.WriteTo.Console(
			theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
			outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {LogType:l} {Message:lj}{NewLine}{Exception}")
		.MinimumLevel.Information()
		.MinimumLevel.Override("CrmBack.Data", context.HostingEnvironment.IsProduction() ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information)
		.MinimumLevel.Override("CrmBack.Api.Middleware", Serilog.Events.LogEventLevel.Warning)
		.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
		.MinimumLevel.Override(
			"Microsoft.EntityFrameworkCore.Database.Command",
			context.HostingEnvironment.IsProduction()
				? Serilog.Events.LogEventLevel.Warning
				: Serilog.Events.LogEventLevel.Information);

	if (!context.HostingEnvironment.IsProduction())
		config.WriteTo.Debug();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Важно для работы с cookies (access_token)
    });
});


builder.Services.AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>()
				 .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>()
				 .AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>()
				 .AddValidatorsFromAssemblyContaining<PaginationDtoValidator>();


builder.Services.AddFluentValidationAutoValidation(config =>
{
	config.DisableDataAnnotationsValidation = true;
});

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICookieService, CookieService>();

builder.Services.AddScoped<IUserDAO, UserDAO>()
			.AddScoped<IActivDAO, ActivDAO>()
			.AddScoped<IOrgDAO, OrgDAO>()
			.AddScoped<IRefreshTokenDAO, RefreshTokenDAO>();

builder.Services.AddScoped<IUserService, UserService>()
			.AddScoped<IActivService, ActivService>()
			.AddScoped<IOrgService, OrgService>()
			.AddScoped<IJwtService, JwtService>();


string jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtIssuer,
			ValidAudience = jwtAudience,
			IssuerSigningKey = new SymmetricSecurityKey(
				System.Text.Encoding.UTF8.GetBytes(jwtKey)),
			ClockSkew = TimeSpan.FromSeconds(30)
		};


		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				if (string.IsNullOrEmpty(context.Token))
				{
					context.Token = context.Request.Cookies["access_token"];
				}
				return Task.CompletedTask;
			}
		};
	});

// Swagger
builder.Services.AddSwaggerGen(option =>
{
	option.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "FARM CRM API",
		Version = "v1",
		Description = "FARM CRM API"
	});
});


builder.Services.AddDbContext<AppDBContext>(op =>
{
	op.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"));
});



// builder.Services.AddResponseCaching();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
	app.UseSerilogRequestLogging();
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

if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

// app.UseResponseCaching();

app.UseCors("Front");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var listenAddress = builder.Environment.IsProduction()
	? "http://0.0.0.0:5555"
	: "http://localhost:5555";

app.Run(listenAddress);

