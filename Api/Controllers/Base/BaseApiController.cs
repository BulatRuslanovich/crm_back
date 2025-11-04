namespace CrmBack.Api.Controllers.Base;

using CrmBack.Api.Filters;
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
}
