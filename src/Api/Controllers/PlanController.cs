
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/plan")]
[Authorize]
public class PlanController(IPlanService service, IDistributedCache cache) : BaseApiController(cache)
{
    private const string EntityPrefix = "plan_";


    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadPlanPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        return await GetDataFromCache(
            $"{EntityPrefix}{id}",
            () => service.GetPlanById(id),
            TimeSpan.FromMinutes(10)
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadPlanPayload>>> GetAll() =>
        Ok(await service.GetAllPlans());

    [HttpPost]
    public async Task<ActionResult<ReadPlanPayload>> Create([FromBody] CreatePlanPayload plan)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await service.CreatePlan(plan);

        if (payload == null) return BadRequest();

        await CleanCache($"{EntityPrefix}{payload.PlanId}");
        return CreatedAtAction(nameof(GetById), new { id = payload.OrgId }, payload);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdatePlanPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid) return BadRequest();

        var updated = await service.UpdatePlan(id, payload);

        if (!updated) return NotFound();

        await CleanCache($"{EntityPrefix}{id}");
        return Ok(true);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        var deleted = await service.DeletePlan(id);

        if (!deleted) return NotFound();

        await CleanCache($"{EntityPrefix}{id}");
        return NoContent();
    }
}