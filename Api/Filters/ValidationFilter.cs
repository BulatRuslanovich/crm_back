using CrmBack.Core.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CrmBack.Api.Filters;

/// <summary>
/// Action filter that intercepts validation errors before action execution
/// Converts FluentValidation errors from ModelState to unified ApiResponse format
/// Applied to all controllers via BaseApiController
/// </summary>
public class ValidationFilter : IActionFilter
{
	/// <summary>
	/// Executes before action: Checks ModelState for validation errors
	/// If invalid, returns BadRequest with unified ApiResponse format including ResponseCode
	/// </summary>
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (!context.ModelState.IsValid)
		{
			// Extract all validation error messages from ModelState
			// FluentValidation populates ModelState with error messages
			var errors = context.ModelState
				.Where(e => e.Value?.Errors.Count > 0)
				.SelectMany(e => e.Value!.Errors.Select(error => error.ErrorMessage))
				.ToList();

			// Create unified error response with ValidationError code and all validation errors
			var errorResponse = ApiResponse<object>.Error(
				ResponseCode.ValidationError,
				"Validation failed. Please check the errors for details.",
				errors
			);

			// Return 422 Unprocessable Entity with error response
			// Prevents action execution when validation fails
			context.Result = new UnprocessableEntityObjectResult(errorResponse);
		}
	}

	/// <summary>
	/// Executes after action: Not used in this filter
	/// </summary>
	public void OnActionExecuted(ActionExecutedContext context)
	{
		// Not needed - validation happens before action execution
	}
}

