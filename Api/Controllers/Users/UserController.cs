using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Api.Controllers.Users;

public class UserController(IUserService userService) : CrudController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService)
{
	protected override int GetId(ReadUserDto payload) => payload.UsrId;

	[HttpGet("{id:int}/activ")]
	public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id, CancellationToken ct = default)
	{
		ValidateId(id);
		var activities = await userService.GetActivs(id, ct);
		return Ok(activities);
	}
}
