namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


[ApiController]
[Route("api/activ")]
[Authorize]
public class ActivController(
    IActivService service,
    IMemoryCache cache,
    ILogger<ActivController> logger) : BaseApiController(cache, logger)
{
    private const string EntityPrefix = "activ_";
    private const string AllCacheKey = "all_activs";


    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadActivPayload>> GetById(int id)
    {
        if (!ValidateId(id, "activity")) return BadRequest("Activity ID must be positive");

        return await GetOrSetCache(
            $"{EntityPrefix}{id}",
            () => service.GetActivById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadActivPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllCacheKey,
            async () =>
            {
                var activs = await service.GetAllActiv();
                return activs.Count != 0 ? activs : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }


    [HttpPost]
    public async Task<ActionResult<ReadActivPayload>> Create([FromBody] CreateActivPayload activ)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await service.CreateActiv(activ);
        if (payload == null)
        {
            logger.LogWarning("Failed to create activity");
            return BadRequest("Failed to create activity");
        }

        logger.LogInformation("Created activity {Id}", payload.ActivId);
        InvalidateCache(payload.ActivId, EntityPrefix, AllCacheKey);
        return CreatedAtAction(nameof(GetById), new { id = payload.ActivId }, payload);
    }


    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateActivPayload payload)
    {
        if (!ValidateId(id, "activity") || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "Activity ID must be positive" : "Invalid data");

        var updated = await service.UpdateActiv(id, payload);
        if (!updated)
        {
            logger.LogWarning("Activity {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated activity {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return Ok(true);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id, "activity")) return BadRequest("Activity ID must be positive");

        var deleted = await service.DeleteActiv(id);
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