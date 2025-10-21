namespace CrmBack.Controllers;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService) : BaseApiController<ReadUserPayload, CreateUserPayload, UpdateUserPayload>(userService)
{

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponsePayload>> Login([FromBody] LoginUserPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await userService.Login(payload, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ReadUserPayload>> Register([FromBody] CreateUserPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await userService.Create(payload, HttpContext.RequestAborted);
            if (response == null) return BadRequest(new { message = "Failed to register user" });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenPayload>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required" });

        try
        {
            var response = await userService.RefreshTokenAsync(request.RefreshToken, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required" });

        try
        {
            await userService.RevokeRefreshTokenAsync(request.RefreshToken, HttpContext.RequestAborted);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/activ")]
    public async Task<ActionResult<List<HumReadActivPayload>>> GetActivs(int id)
    {
        var activs = await userService.GetActivs(id, HttpContext.RequestAborted);
        return Ok(activs);
    }

    [HttpGet("{id:int}/plan")]
    public async Task<ActionResult<List<ReadPlanPayload>>> GetPlans(int id)
    {
        var plans = await userService.GetPlans(id, HttpContext.RequestAborted);
        return Ok(plans);
    }
}
