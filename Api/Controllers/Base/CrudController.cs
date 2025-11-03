namespace CrmBack.Api.Controllers.Base;

using CrmBack.Application.Common.Dto;
using CrmBack.Application.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public abstract class CrudController<RPayload, CPayload, UPayload>(
    IService<RPayload, CPayload, UPayload> service,
    ILogger logger) : BaseApiController
{

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    public async Task<ActionResult<RPayload>> GetById(int id, CancellationToken ct = default)
    {
        ValidateId(id);
        var data = await service.GetById(id, ct);
        return data is null
            ? throw new KeyNotFoundException($"Resource with ID {id} not found")
            : Success(data);
    }

    [HttpGet]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm" })]
    public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] PaginationDto pagination, CancellationToken ct = default) =>
        Success(await service.GetAll(pagination, ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload, CancellationToken ct = default)
    {
        var data = await service.Create(payload, ct);
        if (data is null)
            throw new InvalidOperationException("Failed to create resource");

        var resourceId = GetId(data);
        logger.LogInformation("Resource created: {ResourceId}", resourceId);

        return CreatedAtAction(nameof(GetById), new { id = resourceId }, data);
    }

    protected abstract int GetId(RPayload payload);

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<bool>> Update(int id, [FromBody] UPayload payload, CancellationToken ct = default)
    {
        ValidateId(id);
        var success = await service.Update(id, payload, ct);

        if (!success)
            throw new KeyNotFoundException($"Resource with ID {id} not found or update failed");

        logger.LogInformation("Resource updated: {ResourceId}", id);
        return Success<bool>("Resource updated successfully");
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<string>> Delete(int id, CancellationToken ct = default)
    {
        ValidateId(id);
        var success = await service.Delete(id, ct);

        if (!success)
            throw new KeyNotFoundException($"Resource with ID {id} not found or deletion failed");

        logger.LogInformation("Resource deleted: {ResourceId}", id);
        return Success<string>("Resource deleted successfully");
    }
}
