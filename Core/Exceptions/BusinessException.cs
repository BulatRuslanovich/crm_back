namespace CrmBack.Core.Exceptions;


public class BusinessException(string message, string errorCode = "BUSINESS_ERROR", List<string>? errors = null)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public List<string>? Errors { get; } = errors;
}
