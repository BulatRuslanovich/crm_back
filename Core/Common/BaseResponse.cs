namespace CrmBack.Core.Common;

/// <summary>
/// Unified API response format for all endpoints
/// Ensures consistent response structure across success and error cases
/// Generic type T represents the data payload type
/// </summary>
/// <typeparam name="T">Type of data payload (null for error responses)</typeparam>
/// <param name="Success">Indicates if the request was successful</param>
/// <param name="Data">Response data payload (null for errors)</param>
/// <param name="Message">Optional message describing the response</param>
/// <param name="Errors">List of validation/error messages (for error responses)</param>
/// <param name="Timestamp">UTC timestamp of the response</param>
public record ApiResponse<T>(
    bool Success,
    T? Data = default,
    string? Message = null,
    List<string>? Errors = null,
    DateTime? Timestamp = null
)
{
    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    /// <param name="data">Response data</param>
    /// <param name="message">Optional success message</param>
    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(true, data, message, null, DateTime.UtcNow);

    /// <summary>
    /// Creates a successful response without data (e.g., for delete operations)
    /// </summary>
    /// <param name="message">Optional success message</param>
    public static ApiResponse<T> Ok(string? message = null) =>
        new(true, default, message, null, DateTime.UtcNow);

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errors">Optional list of detailed validation errors</param>
    public static ApiResponse<T> Error(string message, List<string>? errors = null) =>
        new(false, default, message, errors, DateTime.UtcNow);
}
