namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Core.Utils;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
public class AuthController(IUserService userService) : BaseApiController
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto dto)
    {
        if (!ModelState.IsValid) return Error<LoginResponseDto>("Invalid payload", [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]);

        var data = await userService.Login(dto, HttpContext.RequestAborted);
        return data is null ? NotFound<LoginResponseDto>() : Success(data);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid) return Error<ReadUserDto>("Invalid payload", [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]);

        var response = await userService.Create(dto, HttpContext.RequestAborted);
        return response is null
            ? Error<ReadUserDto>("Failed to register user", ["Failed to register user"])
            : Success(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<bool>> RefreshToken()
    {
        bool data = await userService.RefreshToken(ct: HttpContext.RequestAborted);
        return data ? Success(data) : Error<bool>("Failed to refresh token", ["Invalid refresh token"]);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<string>> Logout()
    {
        int? userId = JwtHelper.GetUserIdFromContext(HttpContext);
        if (userId is null) return Error<string>("User not authenticated", ["User not authenticated"]);

        bool success = await userService.Logout(userId.Value, HttpContext.RequestAborted);
        return success
            ? Success<string>("Logged out successfully")
            : Error<string>("Failed to logout", ["Failed to logout"]);
    }
}