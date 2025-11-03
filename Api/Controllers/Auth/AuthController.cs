namespace CrmBack.Api.Controllers.Auth;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Users.Services;
using CrmBack.Core.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
public class AuthController(IUserService userService, ILogger<AuthController> logger) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto dto, CancellationToken ct = default)
    {
        var response = await userService.Login(dto, ct);
        logger.LogInformation("User logged in: {UserId} ({Login})", response.UserId, response.Login);
        return Success(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto dto, CancellationToken ct = default)
    {
        var response = await userService.Create(dto, ct);
        if (response is null)
            throw new InvalidOperationException("Failed to register user. Login might already exist.");

        logger.LogInformation("User registered: {UserId} ({Login})", response.UsrId, response.Login);
        return Success(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(CancellationToken ct = default)
    {
        var response = await userService.RefreshToken(ct: ct);
        logger.LogInformation("Token refreshed for user: {UserId} ({Login})", response.UserId, response.Login);
        return Success(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<string>> Logout(CancellationToken ct = default)
    {
        int? userId = JwtHelper.GetUserIdFromContext(HttpContext);
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated");

        bool success = await userService.Logout(userId.Value, ct);
        if (!success)
            throw new InvalidOperationException("Failed to logout");

        logger.LogInformation("User logged out: {UserId}", userId.Value);
        return Success<string>("Logged out successfully");
    }
}
