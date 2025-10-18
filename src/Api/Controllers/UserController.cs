namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService, IDistributedCache cache) : BaseApiController<ReadUserPayload, CreateUserPayload, UpdateUserPayload>(cache, "user_", userService)
{

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponsePayload>> Login([FromBody] LoginUserPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await userService.Login(payload, HttpContext.RequestAborted);

        if (response == null) return Unauthorized("Invalid username or password");

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ReadUserPayload>> Register([FromBody] CreateUserPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await userService.Create(payload, HttpContext.RequestAborted);

        if (response == null) return BadRequest("Failed to register user");

        return Ok(response);
    }
}