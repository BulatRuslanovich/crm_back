namespace CrmBack.Api.Controllers.Auth;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Users.Services;
using CrmBack.Core.Common;
using CrmBack.Core.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Authentication controller
/// Handles user authentication, registration, token refresh, and logout
/// </summary>
[ApiVersion("1.0")]
public class AuthController(IUserService userService, ILogger<AuthController> logger) : BaseApiController
{
    /// <summary>
    /// POST /auth/login - Authenticate user and return JWT tokens
    /// Security: AllowAnonymous - no authentication required
    /// Tokens are set as HTTP-only cookies for XSS protection
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto dto, CancellationToken ct = default)
    {
        var response = await userService.Login(dto, ct);
        logger.LogInformation("User logged in: {UserId} ({Login})", response.UserId, response.Login);
        return Ok(ApiResponse<LoginResponseDto>.Ok(response));
    }

    /// <summary>
    /// POST /auth/register - Register a new user
    /// Security: AllowAnonymous - no authentication required
    /// Validates user data and creates user account
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto dto, CancellationToken ct = default)
    {
        var response = await userService.Create(dto, ct);
        if (response is null)
            throw new InvalidOperationException("Failed to register user. Login might already exist.");

        logger.LogInformation("User registered: {UserId} ({Login})", response.UsrId, response.Login);
        return Ok(ApiResponse<ReadUserDto>.Ok(response));
    }

    /// <summary>
    /// POST /auth/refresh - Refresh access token using refresh token
    /// Security: Requires authentication (refresh token from cookie)
    /// Issues new access and refresh tokens, invalidates old refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(CancellationToken ct = default)
    {
        var response = await userService.RefreshToken(ct: ct);
        logger.LogInformation("Token refreshed for user: {UserId} ({Login})", response.UserId, response.Login);
        return Ok(ApiResponse<RefreshTokenResponseDto>.Ok(response));
    }

    /// <summary>
    /// POST /auth/logout - Logout current user
    /// Security: Requires authentication
    /// Invalidates refresh token and clears authentication cookies
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<string>> Logout(CancellationToken ct = default)
    {
        // Extract user ID from JWT token in HTTP context
        int? userId = JwtHelper.GetUserIdFromContext(HttpContext);
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated");

        bool success = await userService.Logout(userId.Value, ct);
        if (!success)
            throw new InvalidOperationException("Failed to logout");

        logger.LogInformation("User logged out: {UserId}", userId.Value);
        return Ok(ApiResponse<string>.Ok("Logged out successfully"));
    }
}
