namespace CrmBack.Api.Controllers;

using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Controller for managing organizations.
/// </summary>
[ApiController]
[Route("api/org")]
[Authorize]
public class OrgController(
    IOrgService orgService,
    IMemoryCache cache,
    ILogger<OrgController> logger) : ControllerBase
{
    private const string AllOrgsCacheKey = "all_orgs";

    /// <summary>
    /// Retrieves an organization by its unique identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadOrgPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadOrgPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest("Organization ID must be positive");

        return await GetOrSetCache(
            $"org_{id}",
            () => orgService.GetOrgById(id),
            TimeSpan.FromMinutes(5)
        );
    }

    /// <summary>
    /// Retrieves all organizations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadOrgPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadOrgPayload>>> GetAll()
    {
        return await GetOrSetCache(
            AllOrgsCacheKey,
            async () =>
            {
                var orgs = await orgService.GetAllOrgs();
                return orgs.Count != 0 ? orgs : null;
            },
            TimeSpan.FromMinutes(10)
        );
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
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
        InvalidateCache(payload.OrgId);
        return CreatedAtAction(nameof(GetById), new { id = payload.OrgId }, payload);
    }

    /// <summary>
    /// Updates an existing organization by its unique identifier.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateOrgPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid)
            return BadRequest(id <= 0 ? "Organization ID must be positive" : "Invalid data");

        var updated = await orgService.UpdateOrg(id, payload);
        if (!updated)
        {
            logger.LogWarning("Organization {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated organization {Id}", id);
        InvalidateCache(id);
        return Ok(true);
    }

    /// <summary>
    /// Deletes an organization by its unique identifier.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest("Organization ID must be positive");

        var deleted = await orgService.DeleteOrg(id);
        if (!deleted)
        {
            logger.LogWarning("Organization {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted organization {Id}", id);
        InvalidateCache(id);
        return NoContent();
    }

    // Helper methods

    private bool ValidateId(int id)
    {
        if (id > 0) return true;
        logger.LogWarning("Invalid organization ID {Id}", id);
        return false;
    }

    private void InvalidateCache(int id)
    {
        cache.Remove($"org_{id}");
        cache.Remove(AllOrgsCacheKey);
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
