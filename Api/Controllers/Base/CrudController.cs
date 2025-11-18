namespace CrmBack.Api.Controllers.Base;

using CrmBack.Application.Common.Dto;
using CrmBack.Application.Common.Services;
using Microsoft.AspNetCore.Mvc;

public abstract class CrudController<RPayload, CPayload, UPayload>(
	IService<RPayload, CPayload, UPayload> service) : BaseApiController
{
	[HttpGet("{id:int}")]
	public async Task<ActionResult<RPayload>> GetById(int id, CancellationToken ct = default)
	{
		ValidateId(id);
		var data = await service.GetById(id, ct);
		return data is null ? NotFound() : Ok(data);
	}

	[HttpGet]
	public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] PaginationDto pagination, CancellationToken ct = default) =>
		Ok(await service.GetAll(pagination, ct));

	[HttpPost]
	public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload, CancellationToken ct = default)
	{
		var data = await service.Create(payload, ct);
		if (data is null)
			return BadRequest();

		var resourceId = GetId(data);
		var locationUri = Url.Action(nameof(GetById), new { id = resourceId }) ?? $"/{resourceId}";
		return Created(locationUri, data);
	}

	protected abstract int GetId(RPayload payload);

	[HttpPut("{id:int}")]
	public async Task<ActionResult<RPayload>> Update(int id, [FromBody] UPayload payload, CancellationToken ct = default)
	{
		ValidateId(id);
		var success = await service.Update(id, payload, ct);
		if (!success)
			return NotFound();

		var updatedData = await service.GetById(id, ct);
		return updatedData is null ? NotFound() : Ok(updatedData);
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
	{
		ValidateId(id);
		var success = await service.Delete(id, ct);
		return success ? NoContent() : NotFound();
	}
}
