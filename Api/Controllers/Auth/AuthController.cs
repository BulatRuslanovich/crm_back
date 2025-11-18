namespace CrmBack.Api.Controllers.Auth;

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
		var response = await userService.Login(dto, ct);
		return Ok(response);
	}

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto dto, CancellationToken ct = default)
	{
		var response = await userService.Create(dto, ct);
		if (response is null)
			return Conflict();

		var locationUri = Url.Action(nameof(Login)) ?? "/auth/login";
		return Created(locationUri, response);
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
