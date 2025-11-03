using System.Net;
using CrmBack.Core.Common;
using CrmBack.Core.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CrmBack.Core.Infrastructure.Middleware;

/// <summary>
/// Global middleware for handling exceptions using ApiResponse format
/// </summary>
public class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, response) = ex switch
        {
            ValidationException validationEx => CreateValidationResponse(validationEx),
            BusinessException businessEx => CreateBusinessExceptionResponse(businessEx),
            ArgumentException => ((int)HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(ex.Message, null)),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(ex.Message, null)),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(ex.Message, null)),
            DbUpdateException dbEx => HandleDbException(dbEx),
            OperationCanceledException or TaskCanceledException => ((int)HttpStatusCode.RequestTimeout,
                ApiResponse<object>.Fail("Request timeout", null)),
            _ => ((int)HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(
                    context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                        ? ex.Message
                        : "An error occurred while processing your request",
                    null))
        };

        LogException(ex, context, statusCode);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }

    private static (int StatusCode, ApiResponse<object> Response) CreateValidationResponse(ValidationException ex)
    {
        var errors = ex.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();

        return ((int)HttpStatusCode.BadRequest,
            ApiResponse<object>.Fail("Validation failed", errors));
    }

    private static (int StatusCode, ApiResponse<object> Response) CreateBusinessExceptionResponse(BusinessException ex)
    {
        return ((int)HttpStatusCode.BadRequest,
            ApiResponse<object>.Fail(ex.Message, ex.Errors));
    }

    private static (int StatusCode, ApiResponse<object> Response) HandleDbException(DbUpdateException ex)
    {
        string message = ex.InnerException?.Message.Contains("duplicate key") == true
            ? "Resource already exists"
            : "Database operation failed";

        return ((int)HttpStatusCode.Conflict,
            ApiResponse<object>.Fail(message, null));
    }

    private static void LogException(Exception ex, HttpContext context, int statusCode)
    {
        var level = statusCode >= 500
            ? Serilog.Events.LogEventLevel.Error
            : Serilog.Events.LogEventLevel.Warning;

        Log.Write(level, ex,
            "Exception: {Method} {Path} -> {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            statusCode);
    }
}
