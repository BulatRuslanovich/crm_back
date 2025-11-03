using CrmBack.Core.Extensions;
using CrmBack.Application.Common.Dto;
using CrmBack.Infrastructure.Persistence.Common;
using CrmBack.Application.Common.Specifications;
using CrmBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using CrmBack.Domain.Organizations;
using CrmBack.Application.Organizations.Dto;

namespace CrmBack.Infrastructure.Persistence.Organizations;

public class OrgDAO(AppDBContext context, IConnectionMultiplexer redis) : BaseCrudDAO<OrgEntity, ReadOrgDto, CreateOrgDto, UpdateOrgDto>(context, redis), IOrgDAO
{
    protected override string CacheKeyPrefix => "Org";
    protected override ReadOrgDto MapToDto(OrgEntity entity) => entity.ToReadDto();

    protected override OrgEntity MapToEntity(CreateOrgDto dto) => dto.ToEntity();

    protected override void UpdateEntity(OrgEntity entity, UpdateOrgDto dto)
    {
        entity.Name = dto.Name ?? entity.Name;
        entity.Inn = dto.INN ?? entity.Inn;
        entity.Latitude = dto.Latitude ?? entity.Latitude;
        entity.Longitude = dto.Longitude ?? entity.Longitude;
        entity.Address = dto.Address ?? entity.Address;
    }

    protected override IQueryable<OrgEntity> ApplyDefaults(IQueryable<OrgEntity> query, PaginationDto pagination)
        => query
            .WhereNotDeleted()
            .AsNoTracking()
            .Search(pagination.SearchTerm)
            .OrderByDefault()
            .Paginate(pagination);
}
