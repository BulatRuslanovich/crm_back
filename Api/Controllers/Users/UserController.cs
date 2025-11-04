using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Users.Services;
using CrmBack.Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Api.Controllers.Users;

/// <summary>
/// User management controller
/// Inherits CRUD operations from CrudController and adds user-specific endpoints
/// Security: Requires authentication (all endpoints are protected)
/// </summary>
[ApiVersion("1.0")]
[Authorize]
public class UserController(IUserService userService, ILogger<UserController> logger) : CrudController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService, logger)
{

    /// <summary>
    /// Implements abstract method from CrudController
    /// Extracts user ID from read DTO
    /// </summary>
    protected override int GetId(ReadUserDto payload) => payload.UsrId;

    /// <summary>
    /// GET /users/{id}/activ - Get all activities for a specific user
    /// Performance: Response cached for 60 seconds with cache key based on user ID
    /// Returns list of activities associated with the user
    /// </summary>
    [HttpGet("{id:int}/activ")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id, CancellationToken ct = default)
    {
        ValidateId(id);
        return Ok(ApiResponse<List<HumReadActivDto>>.Ok(await userService.GetActivs(id, ct)));
    }
}
