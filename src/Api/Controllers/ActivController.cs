namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/activ")]
public class ActivController(IActivService activService, IMemoryCache cache, ILogger<ActivController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves an activity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the activity (must be positive).</param>
    /// <returns>The activity details in <see cref="ReadActivPayload"/> format.</returns>
    /// <response code="200">Activity found and returned successfully.</response>
    /// <response code="400">Invalid activity ID provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">Activity with the specified ID not found.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadActivPayload>> GetById(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid activity ID {Id} provided for GetById", id);
            return BadRequest(new { error = "Activity ID must be a positive integer" });
        }

        string cacheKey = $"activ_{id}";
        if (cache.TryGetValue(cacheKey, out ReadActivPayload? activ))
        {
            logger.LogInformation("Retrieved activity {Id} from cache", id);
            return Ok(activ);
        }

        activ = await activService.GetActivById(id);
        if (activ == null)
        {
            logger.LogWarning("Activity with ID {Id} not found", id);
            return NotFound();
        }

        cache.Set(cacheKey, activ, TimeSpan.FromMinutes(5));
        logger.LogInformation("Cached activity {Id}", id);
        return Ok(activ);
    }

    /// <summary>
    /// Retrieves all activities.
    /// </summary>
    /// <returns>A list of all activities in <see cref="ReadActivPayload"/> format.</returns>
    /// <response code="200">List of activities returned successfully.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">No activities found in the system.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<ReadActivPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadActivPayload>>> GetAll()
    {
        const string cacheKey = "all_activs";
        if (cache.TryGetValue(cacheKey, out IEnumerable<ReadActivPayload>? activs))
        {
            logger.LogInformation("Retrieved all activities from cache");
            return Ok(activs);
        }

        activs = await activService.GetAllActiv();
        if (!activs.Any())
        {
            logger.LogWarning("No activities found");
            return NotFound();
        }

        cache.Set(cacheKey, activs, TimeSpan.FromMinutes(10));
        logger.LogInformation("Cached all activities");
        return Ok(activs);
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="activ">The activity data to create, provided in <see cref="CreateActivPayload"/> format.</param>
    /// <returns>The created activity in <see cref="ReadActivPayload"/> format.</returns>
    /// <response code="201">Activity created successfully, returns activity details.</response>
    /// <response code="400">Invalid activity data provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadActivPayload>> Create([FromBody] CreateActivPayload activ)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid activity data provided for Create: {Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var payload = await activService.CreateActiv(activ);
        if (payload == null)
        {
            logger.LogWarning("Failed to create activity");
            return BadRequest(new { error = "Failed to create activity" });
        }

        logger.LogInformation("Created activity with ID {Id}", payload.ActivId);
        return CreatedAtAction(nameof(GetById), new { id = payload.ActivId }, payload);
    }

    /// <summary>
    /// Updates an existing activity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the activity to update (must be positive).</param>
    /// <param name="payload">The updated activity data in <see cref="UpdateActivPayload"/> format.</param>
    /// <returns>True if the activity was updated successfully, otherwise false.</returns>
    /// <response code="200">Activity updated successfully.</response>
    /// <response code="400">Invalid activity ID or data provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">Activity with the specified ID not found.</response>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateActivPayload payload)
    {
        if (id <= 0 || !ModelState.IsValid)
        {
            logger.LogWarning("Invalid activity ID {Id} or data provided for Update", id);
            return BadRequest(new { error = id <= 0 ? "Activity ID must be a positive integer" : "Invalid activity data" });
        }

        var updated = await activService.UpdateActiv(id, payload);
        if (!updated)
        {
            logger.LogWarning("Activity with ID {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated activity with ID {Id}", id);
        cache.Remove($"activ_{id}");
        cache.Remove("all_activs");
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an activity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the activity to delete (must be positive).</param>
    /// <returns>No content if the activity was deleted successfully.</returns>
    /// <response code="204">Activity deleted successfully.</response>
    /// <response code="400">Invalid activity ID provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">Activity with the specified ID not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid activity ID {Id} provided for Delete", id);
            return BadRequest(new { error = "Activity ID must be a positive integer" });
        }

        var deleted = await activService.DeleteActiv(id);
        if (!deleted)
        {
            logger.LogWarning("Activity with ID {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted activity with ID {Id}", id);
        cache.Remove($"activ_{id}");
        cache.Remove("all_activs");
        return NoContent();
    }
}

