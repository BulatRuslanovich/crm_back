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
	/// Maps exception types to appropriate HTTP status codes and ResponseCode enum values
	/// Provides detailed error information including error codes and messages
	/// </summary>
	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		var response = context.Response;
		response.ContentType = "application/json";

		ApiResponse<object> errorResponse;
		int httpStatusCode;

		// Map exception types to HTTP status codes and error responses
		switch (exception)
		{
			case ArgumentException argEx:
				// 400 Bad Request: Invalid input parameters
				httpStatusCode = (int)HttpStatusCode.BadRequest;
				errorResponse = ApiResponse<object>.Error(
					ResponseCode.BadRequest,
					argEx.Message ?? "Invalid argument provided"
				);
				logger.LogWarning(exception, "Argument exception: {Message}", argEx.Message);
				break;

			case KeyNotFoundException keyEx:
				// 404 Not Found: Requested resource doesn't exist
				httpStatusCode = (int)HttpStatusCode.NotFound;
				errorResponse = ApiResponse<object>.Error(
					ResponseCode.NotFound,
					keyEx.Message ?? "Resource not found"
				);
				logger.LogWarning(exception, "Resource not found: {Message}", keyEx.Message);
				break;

			case BusinessException businessEx:
				// 450 Business Error: Business rule violation
				// Supports multiple validation errors via Errors list
				httpStatusCode = (int)HttpStatusCode.BadRequest;
				errorResponse = ApiResponse<object>.Error(
					ResponseCode.BusinessError,
					businessEx.Message ?? "Business rule violation",
					businessEx.Errors
				);
				logger.LogWarning(exception, "Business exception: {Message}, Errors: {Errors}",
					businessEx.Message, businessEx.Errors != null ? string.Join(", ", businessEx.Errors) : "None");
				break;

			case UnauthorizedAccessException unauthorizedEx:
				// 401 Unauthorized: Authentication required
				httpStatusCode = (int)HttpStatusCode.Unauthorized;
				errorResponse = ApiResponse<object>.Error(
					ResponseCode.Unauthorized,
					unauthorizedEx.Message ?? "Unauthorized access. Authentication required."
				);
				logger.LogWarning(exception, "Unauthorized access attempt");
				break;

			case InvalidOperationException invalidOpEx:
				// 409 Conflict: Invalid operation (e.g., resource already exists, duplicate entry)
				httpStatusCode = (int)HttpStatusCode.Conflict;
				errorResponse = ApiResponse<object>.Error(
					ResponseCode.Conflict,
					invalidOpEx.Message ?? "Operation cannot be completed due to conflict"
				);
				logger.LogWarning(exception, "Invalid operation: {Message}", invalidOpEx.Message);
				break;

			default:
				// 500 Internal Server Error: Unexpected exception
				// Security: Don't expose exception details to client (prevent information leakage)
				httpStatusCode = (int)HttpStatusCode.InternalServerError;
				errorResponse = ApiResponse<object>.Error(
					ResponseCode.InternalServerError,
					"An unexpected error occurred while processing your request. Please try again later."
				);
				// Log full exception details server-side for debugging
				logger.LogError(exception, "Unhandled exception occurred: {ExceptionType} - {Message}",
					exception.GetType().Name, exception.Message);
				break;
		}

		response.StatusCode = httpStatusCode;

		// Serialize error response with camelCase naming (JavaScript convention)
		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};

		var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
		await response.WriteAsync(jsonResponse);
	}
}

