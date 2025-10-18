namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService, IDistributedCache cache) : BaseApiController(cache)
{
    private const string EntityPrefix = "user_";

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadUserPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        return await GetDataFromCache(
            $"{EntityPrefix}{id}",
            () => userService.GetUserById(id, HttpContext.RequestAborted),
            TimeSpan.FromMinutes(5)
        );
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ReadUserPayload>>> GetAll([FromQuery] bool isDeleted = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 1000) return BadRequest("Invalid pagination parameters");
        return Ok(await GetDataFromCache(
            $"{EntityPrefix}{isDeleted}_{page}_{pageSize}",
            async () => await userService.GetAllUsers(isDeleted, page, pageSize, HttpContext.RequestAborted),
            TimeSpan.FromMinutes(10)
        ));
    }

    [HttpPost]
    public async Task<ActionResult<ReadUserPayload>> Create([FromBody] CreateUserPayload user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await userService.CreateUser(user, HttpContext.RequestAborted);

        if (payload == null) return BadRequest("Failed to create user");

        await CleanCache($"{EntityPrefix}{payload.Id}");
        return CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateUserPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid) return BadRequest();

        var updated = await userService.UpdateUser(id, payload, HttpContext.RequestAborted);

        if (!updated) return NotFound();

        await CleanCache($"{EntityPrefix}{id}");
        return Ok(true);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        var deleted = await userService.DeleteUser(id, HttpContext.RequestAborted);

        if (!deleted) return NotFound();

        await CleanCache($"{EntityPrefix}{id}");
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponsePayload>> Login([FromBody] LoginUserPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await userService.LoginUser(payload, HttpContext.RequestAborted);

        if (response == null) return Unauthorized("Invalid username or password");


        return Ok(response);
    }
}