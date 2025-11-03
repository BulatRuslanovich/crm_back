namespace CrmBack.Core.Common;

public record ApiResponse<T>(
    string? Message = null,
    T? Data = default,
    DateTime? Timestamp = null
)
{

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(message, data, DateTime.UtcNow);

    public static ApiResponse<T> Ok(string? message = null) =>
        new(message, default, DateTime.UtcNow);

}
