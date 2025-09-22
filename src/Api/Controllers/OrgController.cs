using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/org")]
public class OrgController(IOrgService orgService, IMemoryCache cache, ILogger<OrgController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves an organization by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the organization (must be positive).</param>
    /// <returns>The organization details in <see cref="ReadOrgPayload"/> format.</returns>
    /// <response code="200">Organization found and returned successfully.</response>
    /// <response code="400">Invalid organization ID provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">Organization with the specified ID not found.</response>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadOrgPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadOrgPayload>> GetById(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid organization ID {Id} provided for GetById", id);
            return BadRequest(new { error = "Organization ID must be a positive integer" });
        }

        string cacheKey = $"org_{id}";
        if (cache.TryGetValue(cacheKey, out ReadOrgPayload? org))
        {
            logger.LogInformation("Retrieved organization {Id} from cache", id);
            return Ok(org);
        }

        org = await orgService.GetOrgById(id);
        if (org == null)
        {
            logger.LogWarning("Organization with ID {Id} not found", id);
            return NotFound();
        }

        cache.Set(cacheKey, org, TimeSpan.FromMinutes(5));
        logger.LogInformation("Cached organization {Id}", id);
        return Ok(org);
    }

    /// <summary>
    /// Retrieves all organizations.
    /// </summary>
    /// <returns>A list of all organizations in <see cref="ReadOrgPayload"/> format.</returns>
    /// <response code="200">List of organizations returned successfully.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">No organizations found in the system.</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadOrgPayload>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReadOrgPayload>>> GetAll()
    {
        const string cacheKey = "all_orgs";
        if (cache.TryGetValue(cacheKey, out IEnumerable<ReadOrgPayload>? orgs))
        {
            logger.LogInformation("Retrieved all organizations from cache");
            return Ok(orgs);
        }

        orgs = await orgService.GetAllOrgs();
        if (!orgs.Any())
        {
            logger.LogWarning("No organizations found");
            return NotFound();
        }

        cache.Set(cacheKey, orgs, TimeSpan.FromMinutes(10));
        logger.LogInformation("Cached all organizations");
        return Ok(orgs);
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <param name="org">The organization data to create, provided in <see cref="CreateOrgPayload"/> format.</param>
    /// <returns>The created organization in <see cref="ReadOrgPayload"/> format.</returns>
    /// <response code="201">Organization created successfully, returns organization details.</response>
    /// <response code="400">Invalid organization data provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ReadOrgPayload), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReadOrgPayload>> Create([FromBody] CreateOrgPayload org)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid organization data provided for Create: {Errors}", ModelState);
            return BadRequest(ModelState);
        }

        var payload = await orgService.CreateOrg(org);
        if (payload == null)
        {
            logger.LogWarning("Failed to create organization");
            return BadRequest(new { error = "Failed to create organization" });
        }

        logger.LogInformation("Created organization with ID {Id}", payload.OrgId);
        return CreatedAtAction(nameof(GetById), new { id = payload.OrgId }, payload);
    }

    /// <summary>
    /// Updates an existing organization by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the organization to update (must be positive).</param>
    /// <param name="payload">The updated organization data in <see cref="UpdateOrgPayload"/> format.</param>
    /// <returns>True if the organization was updated successfully, otherwise false.</returns>
    /// <response code="200">Organization updated successfully.</response>
    /// <response code="400">Invalid organization ID or data provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">Organization with the specified ID not found.</response>
    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateOrgPayload payload)
    {
        if (id <= 0 || !ModelState.IsValid)
        {
            logger.LogWarning("Invalid organization ID {Id} or data provided for Update", id);
            return BadRequest(new { error = id <= 0 ? "Organization ID must be a positive integer" : "Invalid organization data" });
        }

        var updated = await orgService.UpdateOrg(id, payload);
        if (!updated)
        {
            logger.LogWarning("Organization with ID {Id} not found for update", id);
            return NotFound();
        }

        logger.LogInformation("Updated organization with ID {Id}", id);
        cache.Remove($"org_{id}");
        cache.Remove("all_orgs");
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an organization by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the organization to delete (must be positive).</param>
    /// <returns>No content if the organization was deleted successfully.</returns>
    /// <response code="204">Organization deleted successfully.</response>
    /// <response code="400">Invalid organization ID provided.</response>
    /// <response code="401">Unauthorized access due to missing or invalid JWT token.</response>
    /// <response code="404">Organization with the specified ID not found.</response>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("Invalid organization ID {Id} provided for Delete", id);
            return BadRequest(new { error = "Organization ID must be a positive integer" });
        }

        var deleted = await orgService.DeleteOrg(id);
        if (!deleted)
        {
            logger.LogWarning("Organization with ID {Id} not found for deletion", id);
            return NotFound();
        }

        logger.LogInformation("Deleted organization with ID {Id}", id);
        cache.Remove($"org_{id}");
        cache.Remove("all_orgs");
        return NoContent();
    }
}
