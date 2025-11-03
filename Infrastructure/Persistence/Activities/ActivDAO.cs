using CrmBack.Core.Extensions;
using CrmBack.Application.Common.Dto;
using CrmBack.Domain.Activities;
using CrmBack.Application.Common.Specifications;
using CrmBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using CrmBack.Infrastructure.Persistence.Common;
using CrmBack.Application.Activities.Dto;

namespace CrmBack.Infrastructure.Persistence.Activities;

public class ActivDAO(AppDBContext context, IConnectionMultiplexer redis) : BaseCrudDAO<ActivEntity, ReadActivDto, CreateActivDto, UpdateActivDto>(context, redis), IActivDAO
{
    protected override string CacheKeyPrefix => "Activ";
    protected override ReadActivDto MapToDto(ActivEntity entity) => entity.ToReadDto();

    protected override ActivEntity MapToEntity(CreateActivDto dto) => dto.ToEntity();

    protected override void UpdateEntity(ActivEntity entity, UpdateActivDto dto)
    {
        entity.StatusId = dto.StatusId ?? entity.StatusId;
        entity.VisitDate = dto.VisitDate ?? entity.VisitDate;
        entity.StartTime = dto.StartTime ?? entity.StartTime;
        entity.EndTime = dto.EndTime ?? entity.EndTime;
        entity.Description = dto.Description ?? entity.Description;
    }

    protected override IQueryable<ActivEntity> ApplyDefaults(IQueryable<ActivEntity> query, PaginationDto pagination)
        => query
            .WhereNotDeleted()
            .AsNoTracking()
            .Search(pagination.SearchTerm)
            .OrderByDefault()
            .Paginate(pagination);

    public async Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default)
    {
        var entities = await Context.Activ
            .WhereNotDeleted()
            .AsNoTracking()
            .Where(a => a.UsrId == userId)
            .OrderByDefault()
            .ToListAsync(ct);
        return entities.Select(MapToDto).ToList();
    }
}
