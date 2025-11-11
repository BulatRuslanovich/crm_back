namespace CrmBack.Core.Exceptions;


public class BusinessException(string message, List<string>? errors = null)
	: Exception(message)
{
	public List<string>? Errors { get; } = errors;
}
