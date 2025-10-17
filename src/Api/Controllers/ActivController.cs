
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.Api.Controllers;

[ApiController]
[Route("api/activ")]
[Authorize]
public class ActivController(IActivService service, IDistributedCache cache) : BaseApiController(cache)
{
    private const string EntityPrefix = "activ_";

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadActivPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        return await GetDataFromCache(
            $"{EntityPrefix}{id}",
            () => service.GetActivById(id, HttpContext.RequestAborted),
            TimeSpan.FromMinutes(10)
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadActivPayload>>> GetAll([FromQuery] bool isDeleted = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 1000) return BadRequest("Invalid pagination parameters");
        return Ok(await service.GetAllActiv(isDeleted, page, pageSize, HttpContext.RequestAborted));
    }

    [HttpPost]
    public async Task<ActionResult<ReadActivPayload>> Create([FromBody] CreateActivPayload activ)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var payload = await service.CreateActiv(activ, HttpContext.RequestAborted);

        if (payload == null) return BadRequest("Not created");

        await CleanCache($"{EntityPrefix}{payload.ActivId}");
        return CreatedAtAction(nameof(GetById), new { id = payload.ActivId }, payload);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UpdateActivPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid)
            return BadRequest();

        var updated = await service.UpdateActiv(id, payload, HttpContext.RequestAborted);
        if (!updated)
        {
            return NotFound();
        }

        await CleanCache($"{EntityPrefix}{id}");
        return Ok(true);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest();

        var deleted = await service.DeleteActiv(id, HttpContext.RequestAborted);

        if (!deleted)
        {
            return NotFound();
        }

        await CleanCache($"{EntityPrefix}{id}");
        return NoContent();
    }
}