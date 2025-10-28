namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Core.Utils;
using CrmBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/usr")]
public class UserController(IUserService userService) : BaseApiController<ReadUserDto, CreateUserDto, UpdateUserDto>(userService)
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            return Ok(await userService.Login(dto, HttpContext.RequestAborted));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> Register([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await userService.Create(dto, HttpContext.RequestAborted);
        return response is null
            ? BadRequest(new { message = "Failed to register user" })
            : Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<bool>> RefreshToken()
    {
        try
        {
            return Ok(await userService.RefreshToken(ct: HttpContext.RequestAborted));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var userId = JwtHelper.GetUserIdFromContext(HttpContext);
        if (userId is null) return Unauthorized(new { message = "User not authenticated" });

        var success = await userService.Logout(userId.Value, HttpContext.RequestAborted);
        return success
            ? Ok(new { message = "Logged out successfully" })
            : BadRequest(new { message = "Failed to logout" });
    }

    [HttpGet("{id:int}/activ")]
    [Authorize]
    public async Task<ActionResult<List<HumReadActivDto>>> GetActivs(int id) =>
        Ok(await userService.GetActivs(id, HttpContext.RequestAborted));
}
