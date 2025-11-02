namespace CrmBack.Controllers;

using CrmBack.Core.Utils;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController() : ControllerBase
{
    /// <summary>
    /// Returns a successful response
    /// </summary>
    protected ActionResult<T> Success<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    /// <summary>
    /// Returns a successful response without data
    /// </summary>
    protected ActionResult<T> Success<T>(string? message = null) =>
        Ok(ApiResponse<T>.Ok(message));

    /// <summary>
    /// Returns an error response
    /// </summary>
    protected ActionResult<T> Error<T>(string message, List<string>? errors = null) =>
        BadRequest(ApiResponse<T>.Fail(message, errors));

    /// <summary>
    /// Returns a not found response
    /// </summary>
    protected ActionResult<T> NotFound<T>(string message = "Resource not found") =>
        NotFound(ApiResponse<T>.Fail(message));

    /// <summary>
    /// Returns an unauthorized response
    /// </summary>
    protected IActionResult Unauthorized(string message = "Unauthorized access") =>
        Unauthorized(ApiResponse<object>.Fail(message));
}
