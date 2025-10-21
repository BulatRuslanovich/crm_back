namespace CrmBack.Controllers;

using CrmBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


public abstract class BaseApiController<RPayload, CPayload, UPayload>(IService<RPayload, CPayload, UPayload> service) : ControllerBase
{
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RPayload>> GetById(int id)
    {
        if (!ValidateId(id)) return BadRequest();
        var data = await service.GetById(id, HttpContext.RequestAborted);
        if (data == null) return NotFound();
        return Ok(data);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] bool isDeleted = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 1000) return BadRequest("Invalid pagination parameters");
        return Ok(await service.GetAll(isDeleted, page, pageSize, searchTerm, HttpContext.RequestAborted));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var data = await service.Create(payload, HttpContext.RequestAborted);

        var idProp = data?.GetType().GetProperty("Id");

        if (idProp == null)
        {
            return Created(string.Empty, data);
        }

        var id = idProp.GetValue(data);
        return CreatedAtAction(nameof(GetById), new { id }, data);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UPayload payload)
    {
        if (!ValidateId(id) || !ModelState.IsValid) return BadRequest();

        try
        {
            var updated = await service.Update(id, payload, HttpContext.RequestAborted);
            if (!updated) return NotFound();
            return Ok(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!ValidateId(id)) return BadRequest();
        var deleted = await service.Delete(id, HttpContext.RequestAborted);
        if (!deleted) return NotFound();
        return NoContent();
    }


    protected bool ValidateId(int id) => id > 0;
}
