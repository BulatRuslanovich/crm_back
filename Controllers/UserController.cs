namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Core.Utils;
using CrmBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
public class UserController(IUserService userService) : CrudController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService)
{
    [HttpGet("{id:int}/activ")]
    [Authorize]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id) =>
        Success(await userService.GetActivs(id, HttpContext.RequestAborted));
}
