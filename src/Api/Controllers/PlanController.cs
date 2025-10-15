namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/plan")]
[Authorize]
public class PlanController(
    IPlanService service,
    IMemoryCache cache,
    ILogger<OrgController> logger) : BaseApiController(cache, logger)
{
    private const string EntityPrefix = "plan_";
    private const string AllCacheKey = "all_plans";


    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadPlanPayload>> GetById(int id)
    {
        if (!ValidateId(id, "organization")) return BadRequest("Organization ID must be positive");

        return await GetOrSetCache(
            $"{EntityPrefix}{id}",
            () => service.GetPlanById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadPlanPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllCacheKey,
            async () =>
            {
                var orgs = await service.GetAllPlans();
                return orgs.Count != 0 ? orgs : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }

    [HttpPost]
    public async Task<ActionResult<ReadPlanPayload>> Create([FromBody] CreatePlanPayload plan)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await service.CreatePlan(plan);
        if (payload == null)
        {
            logger.LogWarning("Failed to create organization");
            return BadRequest("Failed to create organization");
        }

        logger.LogInformation("Created organization {Id}", payload.OrgId);
        InvalidateCache(payload.OrgId, EntityPrefix, AllCacheKey);
        return CreatedAtAction(nameof(GetById), new { id = payload.OrgId }, payload);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdatePlanPayload payload)
    {
        if (!ValidateId(id, "organization") || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "Organization ID must be positive" : "Invalid data");

        var updated = await service.UpdatePlan(id, payload);
        if (!updated)
        {
            logger.LogWarning("Organization {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated organization {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return Ok(true);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id, "organization")) return BadRequest("Organization ID must be positive");

        var deleted = await service.DeletePlan(id);
        if (!deleted)
        {
            logger.LogWarning("Organization {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted organization {Id}", id);
        InvalidateCache(id, EntityPrefix, AllCacheKey);
        return NoContent();
    }
}
