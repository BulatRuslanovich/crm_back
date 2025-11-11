using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Dto;
using CrmBack.Application.Common.Specifications;
using CrmBack.Core.Extensions;
using CrmBack.Domain.Activities;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CrmBack.Infrastructure.Persistence.Activities;

/// <summary>
/// Activities Data Access Object implementation
/// Provides database operations for Activity entities with caching
/// Inherits CRUD operations from BaseCrudDAO
/// </summary>
public class ActivDAO(AppDBContext context, IConnectionMultiplexer redis) : BaseCrudDAO<ActivEntity, ReadActivDto, CreateActivDto, UpdateActivDto>(context, redis), IActivDAO
{
	protected override string CacheKeyPrefix => "Activ";

	/// <summary>Map entity to read DTO</summary>
	protected override ReadActivDto MapToDto(ActivEntity entity) => entity.ToReadDto();

	/// <summary>Map create DTO to entity</summary>
	protected override ActivEntity MapToEntity(CreateActivDto dto) => dto.ToEntity();

	/// <summary>
	/// Update entity properties (partial update - only provided fields)
	/// Null values are ignored (preserves existing values)
	/// </summary>
	protected override void UpdateEntity(ActivEntity entity, UpdateActivDto dto)
	{
		entity.StatusId = dto.StatusId ?? entity.StatusId;
		entity.VisitDate = dto.VisitDate ?? entity.VisitDate;
		entity.StartTime = dto.StartTime ?? entity.StartTime;
		entity.EndTime = dto.EndTime ?? entity.EndTime;
		entity.Description = dto.Description ?? entity.Description;
	}

	/// <summary>
	/// Apply default query filters and pagination
	/// Performance: AsNoTracking() - reduces memory overhead (entities are read-only)
	/// </summary>
	protected override IQueryable<ActivEntity> ApplyDefaults(IQueryable<ActivEntity> query, PaginationDto pagination)
		=> query
			.WhereNotDeleted()           // Filter soft-deleted entities
			.AsNoTracking()             // Performance: Don't track entities (read-only)
			.Search(pagination.SearchTerm)  // Apply search filter
			.OrderByDefault()           // Apply default sorting
			.Paginate(pagination);      // Apply pagination

	/// <summary>
	/// Fetch all activities for a specific user
	/// Performance: Uses AsNoTracking() for read-only query
	/// </summary>
	public async Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default)
	{
		var entities = await Context.Activ
			.WhereNotDeleted()
			.AsNoTracking()             // Performance: Don't track entities (read-only)
			.Where(a => a.UsrId == userId)
			.OrderByDefault()
			.ToListAsync(ct);
		return entities.Select(MapToDto).ToList();
	}
}
