using CrmBack.Core.Extensions;
using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Specifications;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.DAOs.Impl;

public class OrgDAO(AppDBContext context, IDistributedCache cache) : BaseCrudDAO<OrgEntity, ReadOrgDto, CreateOrgDto, UpdateOrgDto>(context, cache), IOrgDAO
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
