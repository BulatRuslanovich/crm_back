namespace CrmBack.Api.Controllers.Base;

using CrmBack.Api.Filters;
using CrmBack.Core.Common;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Base controller for all API controllers
/// Provides common functionality and applies ValidationFilter to all actions
/// All controllers should inherit from this base class
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[ServiceFilter(typeof(ValidationFilter))]
public abstract class BaseApiController : ControllerBase
{
	/// <summary>
	/// Validates that ID is positive integer
	/// Throws ArgumentException if validation fails (caught by ExceptionHandlingMiddleware)
	/// </summary>
	/// <param name="id">ID to validate</param>
	/// <exception cref="ArgumentException">Thrown when ID is less than or equal to 0</exception>
	protected void ValidateId(int id)
	{
		if (id <= 0)
		{
			throw new ArgumentException("ID must be greater than 0", nameof(id));
		}
	}

	// ============================================
	// Success Response Helpers
	// ============================================

	/// <summary>
	/// Returns 200 OK with data payload
	/// </summary>
	protected ActionResult<T> Success<T>(T data, string? message = null) =>
		Ok(ApiResponse<T>.Ok(data, message, ResponseCode.Success));

	/// <summary>
	/// Returns 201 Created with data payload
	/// </summary>
	protected ActionResult<T> Created<T>(T data, string? message = null) =>
		base.Created(string.Empty, ApiResponse<T>.Ok(data, message ?? "Resource created successfully", ResponseCode.Created));

	/// <summary>
	/// Returns 201 Created with data payload and location URI
	/// </summary>
	protected ActionResult<T> Created<T>(string uri, T data, string? message = null) =>
		base.Created(uri, ApiResponse<T>.Ok(data, message ?? "Resource created successfully", ResponseCode.Created));

	/// <summary>
	/// Returns 204 No Content
	/// </summary>
	protected ActionResult<T> NoContent<T>(string? message = null) =>
		Ok(ApiResponse<T>.Ok(message ?? "Operation completed successfully", ResponseCode.NoContent));

	// ============================================
	// Error Response Helpers
	// ============================================

	/// <summary>
	/// Returns 400 Bad Request
	/// </summary>
	protected ActionResult<T> BadRequest<T>(string message, List<string>? errors = null) =>
		base.BadRequest(ApiResponse<T>.Error(ResponseCode.BadRequest, message, errors));

	/// <summary>
	/// Returns 401 Unauthorized
	/// </summary>
	protected ActionResult<T> Unauthorized<T>(string? message = null) =>
		base.Unauthorized(ApiResponse<T>.Error(ResponseCode.Unauthorized, message ?? "Unauthorized access"));

	/// <summary>
	/// Returns 403 Forbidden
	/// </summary>
	protected ActionResult<T> Forbidden<T>(string? message = null) =>
		StatusCode(403, ApiResponse<T>.Error(ResponseCode.Forbidden, message ?? "Access forbidden"));

	/// <summary>
	/// Returns 404 Not Found
	/// </summary>
	protected ActionResult<T> NotFound<T>(string? message = null) =>
		base.NotFound(ApiResponse<T>.Error(ResponseCode.NotFound, message ?? "Resource not found"));

	/// <summary>
	/// Returns 409 Conflict
	/// </summary>
	protected ActionResult<T> Conflict<T>(string? message = null) =>
		base.Conflict(ApiResponse<T>.Error(ResponseCode.Conflict, message ?? "Resource conflict"));

	/// <summary>
	/// Returns 422 Validation Error
	/// </summary>
	protected ActionResult<T> ValidationError<T>(string message, List<string> errors) =>
		UnprocessableEntity(ApiResponse<T>.Error(ResponseCode.ValidationError, message, errors));

	/// <summary>
	/// Returns 450 Business Error
	/// </summary>
	protected ActionResult<T> BusinessError<T>(string message, List<string>? errors = null) =>
		base.BadRequest(ApiResponse<T>.Error(ResponseCode.BusinessError, message, errors));

	/// <summary>
	/// Returns 500 Internal Server Error
	/// </summary>
	protected ActionResult<T> InternalServerError<T>(string? message = null) =>
		StatusCode(500, ApiResponse<T>.Error(ResponseCode.InternalServerError, message ?? "An unexpected error occurred"));
}
