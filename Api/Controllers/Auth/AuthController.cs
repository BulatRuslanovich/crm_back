namespace CrmBack.Api.Controllers.Auth;

using System;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AuthController(IUserService userService) : BaseApiController
{
	[AllowAnonymous]
	[HttpPost("login")]
	public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto dto, CancellationToken ct = default)
	{	
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		try {
			var response = await userService.Login(dto, ct);
			return Ok(response);
		} 
		catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Неверный логин или пароль" });
        }
	}

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto dto, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var response = await userService.Create(dto, ct);
		
		if (response is null) {
			return Conflict(new {message = "Пользователь с таким логином уже существует"});
		}
		
		return Ok(response);
	}

	[HttpPost("refresh")]
	public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(CancellationToken ct = default)
	{
		var response = await userService.RefreshToken(ct: ct);
		return Ok(response);
	}

	// [HttpPost("logout")]
	// public async Task<ActionResult> Logout(CancellationToken ct = default)
	// {
	// 	int? userId = JwtHelper.GetUserIdFromContext(HttpContext);
	// 	if (userId is null)
	// 		return Unauthorized();

	// 	bool success = await userService.Logout(userId.Value, ct);
	// 	return success ? Ok() : BadRequest();
	// }
}
