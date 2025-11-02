namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

public abstract class CrudController<RPayload, CPayload, UPayload>(IService<RPayload, CPayload, UPayload> service) : BaseApiController()
{
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    public async Task<ActionResult<RPayload>> GetById(int id) =>
        id <= 0 ? Error<RPayload>("Invalid ID", ["ID must be greater than 0"]) :
        await GetByIdCore(id);

    private async Task<ActionResult<RPayload>> GetByIdCore(int id)
    {
        var data = await service.GetById(id, HttpContext.RequestAborted);
        return data is null ? NotFound<RPayload>() : Success(data);
    }

    [HttpGet]
    // [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm" })]
    public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] PaginationDto pagination) =>
        !ModelState.IsValid
            ? Error<List<RPayload>>("Invalid pagination parameters", [.. ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)])
            : Success(await service.GetAll(pagination, HttpContext.RequestAborted));

    [HttpPost]
    public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload)
    {
        if (!ModelState.IsValid) return Error<RPayload>("Invalid payload", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList());

        var data = await service.Create(payload, HttpContext.RequestAborted);
        if (data is null) return Error<RPayload>("Failed to create resource", ["Failed to create resource"]);

        object? id = data.GetType().GetProperty("Id")?.GetValue(data);
        return id is null
            ? Created(string.Empty, data)
            : CreatedAtAction(nameof(GetById), new { id }, data);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UPayload payload)
    {
        if (id <= 0 || !ModelState.IsValid) return Error<bool>("Invalid payload", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList());

        return await service.Update(id, payload, HttpContext.RequestAborted)
            ? Success<bool>("Resource updated successfully")
            : NotFound<bool>();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<string>> Delete(int id)
    {
        if (id <= 0) return Error<string>("Invalid ID", ["ID must be greater than 0"]);

        return await service.Delete(id, HttpContext.RequestAborted)
            ? Success<string>("Resource deleted successfully")
            : NotFound<string>();
    }
}
