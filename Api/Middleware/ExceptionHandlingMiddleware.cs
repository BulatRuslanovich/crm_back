using System.Net;
using System.Text.Json;
using CrmBack.Core.Common;
using CrmBack.Core.Exceptions;

namespace CrmBack.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and converts them to unified ApiResponse format
/// Must be registered first in the middleware pipeline to catch exceptions from all middleware/components
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue to next middleware/controller
            await next(context);
        }
        catch (Exception ex)
        {
            // Catch any exception thrown in the pipeline
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by converting them to unified ApiResponse format
    /// Maps exception types to appropriate HTTP status codes
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse<object> errorResponse;

        // Map exception types to HTTP status codes and error responses
        switch (exception)
        {
            case ArgumentException argEx:
                // 400 Bad Request: Invalid input parameters
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.Error(
                    argEx.Message
                );
                break;

            case KeyNotFoundException keyEx:
                // 404 Not Found: Requested resource doesn't exist
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = ApiResponse<object>.Error(
                    keyEx.Message
                );
                break;

            case BusinessException businessEx:
                // 400 Bad Request: Business rule violation
                // Supports multiple validation errors via Errors list
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.Error(
                    businessEx.Message,
                    businessEx.Errors
                );
                break;

            case UnauthorizedAccessException:
                // 401 Unauthorized: Authentication required
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = ApiResponse<object>.Error(
                    "Unauthorized access"
                );
                break;

            case InvalidOperationException invalidOpEx:
                // 400 Bad Request: Invalid operation (e.g., resource already exists)
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ApiResponse<object>.Error(
                    invalidOpEx.Message
                );
                break;

            default:
                // 500 Internal Server Error: Unexpected exception
                // Security: Don't expose exception details to client (prevent information leakage)
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = ApiResponse<object>.Error(
                    "An error occurred while processing your request"
                );
                // Log full exception details server-side for debugging
                logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        // Serialize error response with camelCase naming (JavaScript convention)
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }
}

