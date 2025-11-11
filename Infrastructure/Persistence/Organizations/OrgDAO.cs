using CrmBack.Application.Common.Dto;
using CrmBack.Application.Common.Specifications;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Core.Extensions;
using CrmBack.Domain.Organizations;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CrmBack.Infrastructure.Persistence.Organizations;

/// <summary>
/// Organizations Data Access Object implementation
/// Provides database operations for Organization entities with caching
/// Inherits CRUD operations from BaseCrudDAO
/// </summary>
public class OrgDAO(AppDBContext context, IConnectionMultiplexer redis) : BaseCrudDAO<OrgEntity, ReadOrgDto, CreateOrgDto, UpdateOrgDto>(context, redis), IOrgDAO
{
	protected override string CacheKeyPrefix => "Org";

	/// <summary>Map entity to read DTO</summary>
	protected override ReadOrgDto MapToDto(OrgEntity entity) => entity.ToReadDto();

	/// <summary>Map create DTO to entity</summary>
	protected override OrgEntity MapToEntity(CreateOrgDto dto) => dto.ToEntity();

	/// <summary>
	/// Update entity properties (partial update - only provided fields)
	/// Null values are ignored (preserves existing values)
	/// </summary>
	protected override void UpdateEntity(OrgEntity entity, UpdateOrgDto dto)
	{
		entity.Name = dto.Name ?? entity.Name;
		entity.Inn = dto.INN ?? entity.Inn;
		entity.Latitude = dto.Latitude ?? entity.Latitude;
		entity.Longitude = dto.Longitude ?? entity.Longitude;
		entity.Address = dto.Address ?? entity.Address;
	}

	/// <summary>
	/// Apply default query filters and pagination
	/// Performance: AsNoTracking() - reduces memory overhead (entities are read-only)
	/// </summary>
	protected override IQueryable<OrgEntity> ApplyDefaults(IQueryable<OrgEntity> query, PaginationDto pagination)
		=> query
			.WhereNotDeleted()           // Filter soft-deleted entities
			.AsNoTracking()             // Performance: Don't track entities (read-only)
			.Search(pagination.SearchTerm)  // Apply search filter
			.OrderByDefault()           // Apply default sorting
			.Paginate(pagination);      // Apply pagination
}
