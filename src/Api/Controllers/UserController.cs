namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Controller for managing users and authentication.
/// </summary>
[ApiController]
[Route("api/user")]
public class UserController(
    IUserService userService,
    IMemoryCache cache,
    ILogger<UserController> logger) : ControllerBase
{
    private const string AllUsersCacheKey = "all_users";

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadUserPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest("User ID must be positive");

        return await GetOrSetCache(
            $"user_{id}",
            () => userService.GetUserById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadUserPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadUserPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllUsersCacheKey,
            async () =>
            {
                var users = await userService.GetAllUsers();
                return users.Any() ? users : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
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
        InvalidateCache(payload.Id);
        return CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload);
    }

    /// <summary>
    /// Updates an existing user by their unique identifier.
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateUserPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "User ID must be positive" : "Invalid data");

        var updated = await userService.UpdateUser(id, payload);
        if (!updated)
        {
            logger.LogWarning("User {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated user {Id}", id);
        InvalidateCache(id);
        return Ok(true);
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest("User ID must be positive");

        var deleted = await userService.DeleteUser(id);
        if (!deleted)
        {
            logger.LogWarning("User {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted user {Id}", id);
        InvalidateCache(id);
        return NoContent();
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
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

    // Helper methods

    private bool ValidateId(int id)
    {
        if (id > 0) return true;
        logger.LogWarning("Invalid user ID {Id}", id);
        return false;
    }

    private void InvalidateCache(int id)
    {
        cache.Remove($"user_{id}");
        cache.Remove(AllUsersCacheKey);
    }

    private async Task<ActionResult<T>> GetOrSetCache<T>(
        string cacheKey,
        Func<Task<T?>> fetchData,
        TimeSpan expiration) where T : class
    {
        if (cache.TryGetValue(cacheKey, out T? cached))
        {
            logger.LogDebug("Cache hit: {Key}", cacheKey);
            return Ok(cached);
        }

        var data = await fetchData();
        if (data == null)
        {
            logger.LogWarning("Data not found for key: {Key}", cacheKey);
            return NotFound();
        }

        cache.Set(cacheKey, data, expiration);
        logger.LogDebug("Cached: {Key}", cacheKey);
        return Ok(data);
    }
}
