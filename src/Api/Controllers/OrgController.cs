namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/org")]
[Authorize]
public class OrgController(
    IOrgService orgService,
    IMemoryCache cache,
    ILogger<OrgController> logger) : BaseApiController(cache, logger)
{
    private const string EntityPrefix = "org_";
    private const string AllCacheKey = "all_orgs";


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadOrgPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadOrgPayload>> GetById(int id)
    {
        if (!ValidateId(id, "organization")) return BadRequest("Organization ID must be positive");

        return await GetOrSetCache(
            $"{EntityPrefix}{id}",
            () => orgService.GetOrgById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReadOrgPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadOrgPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllCacheKey,
            async () =>
            {
                var orgs = await orgService.GetAllOrgs();
                return orgs.Count != 0 ? orgs : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadOrgPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadOrgPayload>> Create([FromBody] CreateOrgPayload org)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await orgService.CreateOrg(org);
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
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateOrgPayload payload)
    {
        if (!ValidateId(id, "organization") || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "Organization ID must be positive" : "Invalid data");

        var updated = await orgService.UpdateOrg(id, payload);
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id, "organization")) return BadRequest("Organization ID must be positive");

        var deleted = await orgService.DeleteOrg(id);
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
