using System.Net;
using System.Text.Json;
using CrmBack.Core.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CrmBack.Core.Utils.Middleware;

/// <summary>
/// Упрощенный глобальный middleware для обработки исключений
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
        var (statusCode, message, errorCode, errors) = ex switch
        {
            BusinessException e => ((int)HttpStatusCode.BadRequest, e.Message, e.ErrorCode, e.Errors),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message, "NOT_FOUND", null),
            ValidationException e => HandleValidationError(e),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, ex.Message, "UNAUTHORIZED", null),
            DbUpdateException => HandleDbError(ex),
            OperationCanceledException or TaskCanceledException => ((int)HttpStatusCode.RequestTimeout, "Request timeout", "TIMEOUT", null),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal server error", "INTERNAL_ERROR", null)
        };

        LogException(ex, context, statusCode);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message,
            errorCode,
            errors,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private static (int StatusCode, string Message, string ErrorCode, object? Errors) HandleValidationError(FluentValidation.ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());

        return ((int)HttpStatusCode.BadRequest, "Validation failed", "VALIDATION_ERROR", errors);
    }

    private static (int StatusCode, string Message, string ErrorCode, object? Errors) HandleDbError(Exception ex)
    {
        string message = ex.InnerException?.Message.Contains("duplicate key") == true
            ? "Resource already exists"
            : "Database operation failed";

        return ((int)HttpStatusCode.Conflict, message, "DATABASE_ERROR", null);
    }

    private static void LogException(Exception ex, HttpContext context, int statusCode)
    {
        var level = statusCode >= 500 ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Warning;

        Log.Write(level, ex,
            "Exception: {Method} {Path} -> {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            statusCode);
    }
}
