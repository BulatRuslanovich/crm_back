using CrmBack.Application.Common.Dto;

namespace CrmBack.Application.Common.Services;

/// <summary>
/// Generic service interface for CRUD operations
/// Defines standard contract for service layer operations
/// </summary>
/// <typeparam name="ReadDto">DTO returned from read operations</typeparam>
/// <typeparam name="CreateDto">DTO used for creating entities</typeparam>
/// <typeparam name="UpdateDto">DTO used for updating entities</typeparam>
public interface IService<ReadDto, CreateDto, UpdateDto>
{
	/// <summary>Retrieve a single entity by ID</summary>
	public Task<ReadDto?> GetById(int id, CancellationToken ct = default);

	/// <summary>Retrieve all entities with pagination and search</summary>
	public Task<List<ReadDto>> GetAll(PaginationDto pagination, CancellationToken ct = default);

	/// <summary>Create a new entity</summary>
	public Task<ReadDto?> Create(CreateDto payload, CancellationToken ct = default);

	/// <summary>Update an existing entity</summary>
	public Task<bool> Update(int id, UpdateDto payload, CancellationToken ct = default);

	/// <summary>Soft delete an entity (sets IsDeleted flag)</summary>
	public Task<bool> Delete(int id, CancellationToken ct = default);
}
