namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


// TODO: Add logic with cancel token

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService, IMemoryCache cache, ILogger<UserController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user (must be positive).</param>
    /// <returns>The user details in <see cref="ReadUserPayload"/> format.</returns>
    /// <response code="200">User found and returned successfully.</response>
    /// <response code="400">Invalid user ID provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">User with the specified ID not found.</response>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadUserPayload>> GetById(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid user ID {Id} provided for GetById", id);
            return BadRequest(new { error = "User ID must be a positive integer" });
        }

        string cacheKey = $"user_{id}";
        if (cache.TryGetValue(cacheKey, out ReadUserPayload? user))
        {
            logger.LogInformation("Retrieved user {Id} from cache", id);
            return Ok(user);
        }

        user = await userService.GetUserById(id);
        if (user == null)
        {
            logger.LogWarning("User with ID {Id} not found", id);
            return NotFound();
        }

        cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
        logger.LogInformation("Cached user {Id}", id);
        return Ok(user);
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A list of all users in <see cref="ReadUserPayload"/> format.</returns>
    /// <response code="200">List of users returned successfully.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">No users found in the system.</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadUserPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadUserPayload>>> GetAll()
    {
        const string cacheKey = "all_users";
        if (cache.TryGetValue(cacheKey, out IEnumerable<ReadUserPayload>? users))
        {
            logger.LogInformation("Retrieved all users from cache");
            return Ok(users);
        }

        users = await userService.GetAllUsers();
        if (!users.Any())
        {
            logger.LogWarning("No users found");
            return NotFound();
        }

        cache.Set(cacheKey, users, TimeSpan.FromMinutes(10));
        logger.LogInformation("Cached all users");
        return Ok(users);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user data to create, provided in <see cref="CreateUserPayload"/> format.</param>
    /// <returns>The created user in <see cref="ReadUserPayload"/> format.</returns>
    /// <response code="201">User created successfully, returns user details.</response>
    /// <response code="400">Invalid user data provided.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadUserPayload>> Create([FromBody] CreateUserPayload user)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid user data provided for Create: {Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var payload = await userService.CreateUser(user);
        if (payload == null)
        {
            logger.LogWarning("Failed to create user");
            return BadRequest(new { error = "Failed to create user" });
        }

        logger.LogInformation("Created user with ID {Id}", payload.Id);
        return CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload);
    }

    /// <summary>
    /// Updates an existing user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update (must be positive).</param>
    /// <param name="payload">The updated user data in <see cref="UpdateUserPayload"/> format.</param>
    /// <returns>True if the user was updated successfully, otherwise false.</returns>
    /// <response code="200">User updated successfully.</response>
    /// <response code="400">Invalid user ID or data provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">User with the specified ID not found.</response>
    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadUserPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateUserPayload payload)
    {
        if (id <= 0 || !ModelState.IsValid)
        {
            logger.LogWarning("Invalid user ID {Id} or data provided for Update", id);
            return BadRequest(new { error = id <= 0 ? "User ID must be a positive integer" : "Invalid user data" });
        }

        var updated = await userService.UpdateUser(id, payload);
        if (!updated)
        {
            logger.LogWarning("User with ID {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated user with ID {Id}", id);
        cache.Remove($"user_{id}");
        cache.Remove("all_users");
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete (must be positive).</param>
    /// <returns>No content if the user was deleted successfully.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="400">Invalid user ID provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">User with the specified ID not found.</response>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid user ID {Id} provided for Delete", id);
            return BadRequest(new { error = "User ID must be a positive integer" });
        }

        var deleted = await userService.DeleteUser(id);
        if (!deleted)
        {
            logger.LogWarning("User with ID {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted user with ID {Id}", id);
        cache.Remove($"user_{id}");
        cache.Remove("all_users");
        return NoContent();
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="payload">The login credentials in <see cref="LoginUserPayload"/> format.</param>
    /// <returns>A <see cref="LoginResponsePayload"/> containing the JWT token and user details.</returns>
    /// <response code="200">User authenticated successfully, returns JWT token.</response>
    /// <response code="400">Invalid login credentials provided.</response>
    /// <response code="401">Authentication failed due to incorrect credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponsePayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponsePayload>> Login([FromBody] LoginUserPayload payload)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid login data provided: {Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var response = await userService.LoginUser(payload);
        if (response == null)
        {
            logger.LogWarning("Authentication failed for user with payload: {Payload}", payload);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        logger.LogInformation("User authenticated successfully: {UserId}", response.user.Id);
        return Ok(response);
    }
}
