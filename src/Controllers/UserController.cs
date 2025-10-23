namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Core.Utils;
using CrmBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/usr")]
[EnableRateLimiting("AuthenticatedPolicy")]
public class UserController(IUserService userService) : BaseApiController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService)
{
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto Dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await userService.Login(Dto, HttpContext, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [EnableRateLimiting("RegisterPolicy")]
    public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto Dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await userService.Create(Dto, HttpContext.RequestAborted);
            if (response == null) return BadRequest(new { message = "Failed to register user" });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("RefreshPolicy")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken()
    {
        try
        {
            var response = await userService.RefreshToken("", HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var success = await userService.RevokeToken(request.RefreshToken, HttpContext.RequestAborted);
            if (!success) return BadRequest(new { message = "Failed to revoke token" });
            return Ok(new { message = "Token revoked successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = JwtHelper.GetUserIdFromContext(HttpContext);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var success = await userService.Logout(userId.Value, HttpContext.RequestAborted);
            if (!success)
            {
                return BadRequest(new { message = "Failed to logout" });
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("{id:int}/activ")]
    public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id)
    {
        var activs = await userService.GetActivs(id, HttpContext.RequestAborted);
        return Ok(activs);
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<List<ActiveSessionDto>>> GetActiveSessions()
    {
        try
        {
            var userId = JwtHelper.GetUserIdFromContext(HttpContext);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var sessions = await userService.GetActiveSessions(userId.Value, HttpContext.RequestAborted);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("sessions/{sessionId:int}")]
    [Authorize]
    public async Task<ActionResult> RevokeSession(int sessionId)
    {
        try
        {
            var userId = JwtHelper.GetUserIdFromContext(HttpContext);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var success = await userService.RevokeSession(userId.Value, sessionId, HttpContext.RequestAborted);
            if (!success)
            {
                return BadRequest(new { message = "Failed to revoke session" });
            }

            return Ok(new { message = "Session revoked successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
