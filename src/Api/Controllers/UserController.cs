namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/user")]
public class UserController(
    IUserService userService,
    IMemoryCache cache,
    ILogger<UserController> logger) : BaseApiController(cache, logger)
{
    private const string EntityPrefix = "user_";
    private const string AllCacheKey = "all_users";

    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadUserPayload>> GetById(int id)
    {
        if (!ValidateId(id, "user")) return BadRequest("User ID must be positive");

        return await GetOrSetCache(
            $"{EntityPrefix}{id}",
            () => userService.GetUserById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadUserPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadUserPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllCacheKey,
            async () =>
            {
                var users = await userService.GetAllUsers();
                return users.Any() ? users : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadUserPayload>> Create([FromBody] CreateUserPayload user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await userService.CreateUser(user);
        if (payload == null)
        {
            logger.LogWarning("Failed to create user");
            return BadRequest("Failed to create user");
        }

        logger.LogInformation("Created user {Id}", payload.Id);
        InvalidateCache(payload.Id, EntityPrefix, AllCacheKey);
        return CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateUserPayload payload)
    {
        if (!ValidateId(id, "user") || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "User ID must be positive" : "Invalid data");

        var updated = await userService.UpdateUser(id, payload);
        if (!updated)
        {
            logger.LogWarning("User {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated user {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return Ok(true);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id, "user")) return BadRequest("User ID must be positive");

        var deleted = await userService.DeleteUser(id);
        if (!deleted)
        {
            logger.LogWarning("User {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted user {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponsePayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponsePayload>> Login([FromBody] LoginUserPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await userService.LoginUser(payload);
        if (response == null)
        {
            logger.LogWarning("Authentication failed for login: {Login}", payload.Login);
            return Unauthorized("Invalid username or password");
        }

        logger.LogInformation("User authenticated: {UserId}", response.user.Id);
        return Ok(response);
    }
}
