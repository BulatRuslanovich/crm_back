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
        if (id <= 0) return BadRequest();
        
        var data = await service.GetById(id, HttpContext.RequestAborted);
        return data is null ? NotFound() : Ok(data);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] bool isDeleted = false, 
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        if (page < 1 || pageSize is < 1 or > 1000) 
            return BadRequest("Invalid pagination parameters");
        
        return Ok(await service.GetAll(isDeleted, page, pageSize, searchTerm, HttpContext.RequestAborted));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var data = await service.Create(payload, HttpContext.RequestAborted);
        if (data is null) return BadRequest();

        var id = data.GetType().GetProperty("Id")?.GetValue(data);
        return id is null 
            ? Created(string.Empty, data)
            : CreatedAtAction(nameof(GetById), new { id }, data);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UPayload payload)
    {
        if (id <= 0 || !ModelState.IsValid) return BadRequest();

        try
        {
            return await service.Update(id, payload, HttpContext.RequestAborted)
                ? Ok(true)
                : NotFound();
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
        if (id <= 0) return BadRequest();
        
        return await service.Delete(id, HttpContext.RequestAborted)
            ? NoContent()
            : NotFound();
    }
}
