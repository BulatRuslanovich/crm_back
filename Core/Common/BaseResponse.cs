namespace CrmBack.Core.Common;

/// <summary>
/// Unified API response format for all endpoints
/// Ensures consistent response structure across success and error cases
/// Generic type T represents the data payload type
/// </summary>
/// <typeparam name="T">Type of data payload (null for error responses)</typeparam>
/// <param name="Code">Response code indicating operation result (HTTP status code or custom code)</param>
/// <param name="Success">Indicates if the request was successful</param>
/// <param name="Message">Optional message describing the response</param>
/// <param name="Payload">Response data payload (null for errors)</param>
/// <param name="Errors">List of validation/error messages (for error responses)</param>
/// <param name="Timestamp">UTC timestamp of the response</param>
public record ApiResponse<T>(
	ResponseCode Code,
	bool Success,
	string? Message = null,
	T? Payload = default,
	List<string>? Errors = null,
	DateTime? Timestamp = null
)
{
	// ============================================
	// Response Factory Methods
	// ============================================

	/// <summary>
	/// Creates a successful response with data
	/// </summary>
	/// <param name="data">Response data</param>
	/// <param name="message">Optional success message</param>
	/// <param name="code">Response code (defaults to Success)</param>
	public static ApiResponse<T> Ok(T data, string? message = null, ResponseCode code = ResponseCode.Success) =>
		new(code, true, message, data, null, DateTime.UtcNow);

	/// <summary>
	/// Creates a successful response without data (e.g., for delete operations)
	/// </summary>
	/// <param name="message">Optional success message</param>
	/// <param name="code">Response code (defaults to Success)</param>
	public static ApiResponse<T> Ok(string? message = null, ResponseCode code = ResponseCode.Success) =>
		new(code, true, message, default, null, DateTime.UtcNow);

	/// <summary>
	/// Creates an error response
	/// </summary>
	/// <param name="code">Error code indicating the type of error</param>
	/// <param name="message">Error message</param>
	/// <param name="errors">Optional list of detailed validation errors</param>
	public static ApiResponse<T> Error(ResponseCode code, string message, List<string>? errors = null) =>
		new(code, false, message, default, errors, DateTime.UtcNow);

	/// <summary>
	/// Creates an error response with default BadRequest code
	/// </summary>
	/// <param name="message">Error message</param>
	/// <param name="errors">Optional list of detailed validation errors</param>
	public static ApiResponse<T> Error(string message, List<string>? errors = null) =>
		new(ResponseCode.BadRequest, false, message, default, errors, DateTime.UtcNow);
}
