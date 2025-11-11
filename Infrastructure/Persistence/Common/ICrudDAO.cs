using CrmBack.Application.Common.Dto;

namespace CrmBack.Infrastructure.Persistence.Common;

/// <summary>
/// Generic Data Access Object interface for CRUD operations
/// Defines contract for database persistence layer
/// </summary>
/// <typeparam name="ReadDto">DTO returned from read operations</typeparam>
/// <typeparam name="CreateDto">DTO used for creating entities</typeparam>
/// <typeparam name="UpdateDto">DTO used for updating entities</typeparam>
public interface ICrudDAO<ReadDto, CreateDto, UpdateDto>
{
	/// <summary>Fetch all entities with pagination and search</summary>
	public Task<List<ReadDto>> FetchAll(PaginationDto pagination, CancellationToken ct = default);

	/// <summary>Fetch a single entity by ID</summary>
	public Task<ReadDto?> FetchById(int id, CancellationToken ct);

	/// <summary>Create a new entity in database</summary>
	public Task<ReadDto?> Create(CreateDto dto, CancellationToken ct = default);

	/// <summary>Update an existing entity</summary>
	public Task<bool> Update(int id, UpdateDto dto, CancellationToken ct = default);

	/// <summary>Soft delete an entity (sets IsDeleted flag)</summary>
	public Task<bool> Delete(int id, CancellationToken ct = default);
}
