
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/org")]
[Authorize]
public class OrgController(IOrgService orgService, IDistributedCache cache) : BaseApiController(cache)
{
    private const string EntityPrefix = "org_";

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadOrgPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        return await GetDataFromCache(
            $"{EntityPrefix}{id}",
            () => orgService.GetOrgById(id),
            TimeSpan.FromMinutes(10)
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadOrgPayload>>> GetAll() =>
        Ok(await orgService.GetAllOrgs());

    [HttpPost]
    public async Task<ActionResult<ReadOrgPayload>> Create([FromBody] CreateOrgPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var readPayload = await orgService.CreateOrg(payload);

        if (readPayload == null) return BadRequest("Not created");

        await CleanCache($"{EntityPrefix}{readPayload.OrgId}");
        return CreatedAtAction(nameof(GetById), new { id = readPayload.OrgId }, readPayload);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateOrgPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid) return BadRequest(id <= 0);

        var updated = await orgService.UpdateOrg(id, payload);

        if (!updated) return NotFound();

        await CleanCache($"{EntityPrefix}{id}");
        return Ok(true);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        var deleted = await orgService.DeleteOrg(id);

        if (!deleted) return NotFound();

        await CleanCache($"{EntityPrefix}{id}");
        return NoContent();
    }
}