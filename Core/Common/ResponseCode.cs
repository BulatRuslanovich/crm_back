namespace CrmBack.Core.Common;

/// <summary>
/// Unified response codes for API operations
/// Maps to HTTP status codes and provides application-specific error codes
/// Used in ApiResponse to indicate operation result
/// </summary>
public enum ResponseCode
{
	// Success codes (2xx)
	/// <summary>Success: Operation completed successfully</summary>
	Success = 200,

	/// <summary>Created: Resource successfully created</summary>
	Created = 201,

	/// <summary>No Content: Operation successful, no data to return</summary>
	NoContent = 204,

	// Client error codes (4xx)
	/// <summary>Bad Request: Invalid input or validation failed</summary>
	BadRequest = 400,

	/// <summary>Unauthorized: Authentication required</summary>
	Unauthorized = 401,

	/// <summary>Forbidden: Insufficient permissions</summary>
	Forbidden = 403,

	/// <summary>Not Found: Resource does not exist</summary>
	NotFound = 404,

	/// <summary>Conflict: Resource already exists or business rule conflict</summary>
	Conflict = 409,

	/// <summary>Validation Error: Input validation failed</summary>
	ValidationError = 422,

	/// <summary>Business Rule Violation: Business logic constraint violated</summary>
	BusinessError = 450,

	// Server error codes (5xx)
	/// <summary>Internal Server Error: Unexpected error occurred</summary>
	InternalServerError = 500,

	/// <summary>Service Unavailable: Service temporarily unavailable</summary>
	ServiceUnavailable = 503
}
