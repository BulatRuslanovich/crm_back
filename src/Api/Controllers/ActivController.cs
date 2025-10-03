namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Controller for managing activities.
/// </summary>
[ApiController]
[Route("api/activ")]
[Authorize]
public class ActivController(
    IActivService activService,
    IMemoryCache cache,
    ILogger<ActivController> logger) : BaseApiController(cache, logger)
{
    private const string EntityPrefix = "activ_";
    private const string AllCacheKey = "all_activs";

    /// <summary>
    /// Retrieves an activity by its unique identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadActivPayload>> GetById(int id)
    {
        if (!ValidateId(id, "activity")) return BadRequest("Activity ID must be positive");

        return await GetOrSetCache(
            $"{EntityPrefix}{id}",
            () => activService.GetActivById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    /// <summary>
    /// Retrieves all activities.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadActivPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadActivPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllCacheKey,
            async () =>
            {
                var activs = await activService.GetAllActiv();
                return activs.Count != 0 ? activs : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReadActivPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadActivPayload>> Create([FromBody] CreateActivPayload activ)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await activService.CreateActiv(activ);
        if (payload == null)
        {
            logger.LogWarning("Failed to create activity");
            return BadRequest("Failed to create activity");
        }

        logger.LogInformation("Created activity {Id}", payload.ActivId);
        InvalidateCache(payload.ActivId, EntityPrefix, AllCacheKey);
        return CreatedAtAction(nameof(GetById), new { id = payload.ActivId }, payload);
    }

    /// <summary>
    /// Updates an existing activity by its unique identifier.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateActivPayload payload)
    {
        if (!ValidateId(id, "activity") || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "Activity ID must be positive" : "Invalid data");

        var updated = await activService.UpdateActiv(id, payload);
        if (!updated)
        {
            logger.LogWarning("Activity {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated activity {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return Ok(true);
    }

    /// <summary>
    /// Deletes an activity by its unique identifier.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id, "activity")) return BadRequest("Activity ID must be positive");

        var deleted = await activService.DeleteActiv(id);
        if (!deleted)
        {
            logger.LogWarning("Activity {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted activity {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return NoContent();
    }
}
