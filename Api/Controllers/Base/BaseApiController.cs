namespace CrmBack.Api.Controllers.Base;

using CrmBack.Core.Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<T> Success<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<T> Success<T>(string? message = null) =>
        Ok(ApiResponse<T>.Ok(message));

    protected void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID must be greater than 0", nameof(id));
        }
    }
}
