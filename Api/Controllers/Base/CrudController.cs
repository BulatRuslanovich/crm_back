namespace CrmBack.Api.Controllers.Base;

using CrmBack.Application.Common.Dto;
using CrmBack.Application.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Generic CRUD controller with common operations
/// Provides standard Create, Read, Update, Delete endpoints
/// Uses generics to avoid code duplication across entity types
/// </summary>
/// <typeparam name="RPayload">Read DTO - returned from GET operations</typeparam>
/// <typeparam name="CPayload">Create DTO - received in POST operations</typeparam>
/// <typeparam name="UPayload">Update DTO - received in PUT operations</typeparam>
public abstract class CrudController<RPayload, CPayload, UPayload>(
	IService<RPayload, CPayload, UPayload> service,
	ILogger logger) : BaseApiController
{

	/// <summary>
	/// GET /{id} - Retrieve a single resource by ID
	/// Performance: Response cached for 120 seconds with cache key based on ID
	/// </summary>
	[HttpGet("{id:int}")]
	[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
	public async Task<ActionResult<RPayload>> GetById(int id, CancellationToken ct = default)
	{
		ValidateId(id);
		var data = await service.GetById(id, ct);
		// Return 404 if resource not found, otherwise return resource
		return data is null
			? throw new KeyNotFoundException($"Resource with ID {id} not found")
			: Success(data);
	}

	/// <summary>
	/// GET / - Retrieve all resources with pagination
	/// Performance: Response cached for 120 seconds, cache varies by pagination parameters
	/// </summary>
	[HttpGet]
	[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm" })]
	public async Task<ActionResult<List<RPayload>>> GetAll([FromQuery] PaginationDto pagination, CancellationToken ct = default) =>
		Success(await service.GetAll(pagination, ct));

	/// <summary>
	/// POST / - Create a new resource
	/// Security: Requires authentication (Authorize attribute)
	/// Returns 201 Created with Location header pointing to created resource
	/// </summary>
	[HttpPost]
	[Authorize]
	public async Task<ActionResult<RPayload>> Create([FromBody] CPayload payload, CancellationToken ct = default)
	{
		var data = await service.Create(payload, ct);
		if (data is null)
			throw new InvalidOperationException("Failed to create resource");

		var resourceId = GetId(data);
		logger.LogInformation("Resource created: {ResourceId}", resourceId);

		// Return 201 Created with Location header for RESTful compliance
		return Created(
			Url.Action(nameof(GetById), new { id = resourceId }) ?? $"/{resourceId}",
			data
		);
	}

	/// <summary>
	/// Abstract method to extract ID from read DTO
	/// Must be implemented by derived controllers to specify ID property
	/// </summary>
	protected abstract int GetId(RPayload payload);

	/// <summary>
	/// PUT /{id} - Update an existing resource
	/// Security: Requires authentication
	/// </summary>
	[HttpPut("{id:int}")]
	[Authorize]
	public async Task<ActionResult<bool>> Update(int id, [FromBody] UPayload payload, CancellationToken ct = default)
	{
		ValidateId(id);
		var success = await service.Update(id, payload, ct);

		if (!success)
			throw new KeyNotFoundException($"Resource with ID {id} not found or update failed");

		logger.LogInformation("Resource updated: {ResourceId}", id);
		return Success(true, "Resource updated successfully");
	}

	/// <summary>
	/// DELETE /{id} - Delete a resource
	/// Security: Requires authentication
	/// </summary>
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
