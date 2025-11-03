using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Users.Services;
using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Activities.Dto;

namespace CrmBack.Api.Controllers.Users;

[ApiVersion("1.0")]
[Authorize]
public class UserController(IUserService userService, ILogger<UserController> logger) : CrudController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService, logger)
{

    protected override int GetId(ReadUserDto payload) => payload.UsrId;

    [HttpGet("{id:int}/activ")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id, CancellationToken ct = default)
    {
        ValidateId(id);
        return Success(await userService.GetActivs(id, ct));
    }
}
