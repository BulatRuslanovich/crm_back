namespace CrmBack.Core.Common;

public record ApiResponse<T>(
    bool Success,
    string? Message = null,
    T? Data = default,
    List<string>? Errors = null,
    DateTime? Timestamp = null
)
{

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(true, message, data, null, DateTime.UtcNow);

    public static ApiResponse<object> Ok(string? message = null) =>
        new(true, message, null, null, DateTime.UtcNow);

    public static ApiResponse<object> Fail(string message, List<string>? errors = null) =>
        new(false, message, null, errors, DateTime.UtcNow);
}
