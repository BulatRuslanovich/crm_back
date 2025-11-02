using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class OrgDAO(AppDBContext context) : IOrgDAO
{
    public async Task<ReadOrgDto?> Create(CreateOrgDto dto, CancellationToken ct = default)
    {
        var entity = dto.ToEntity();
        context.Org.Add(entity);
        await context.SaveChangesAsync(ct);

        return entity.ToReadDto();
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.Org.FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadOrgDto>> FetchAll(PaginationDto pagination, CancellationToken ct = default)
    {
        var query = context.Org.AsQueryable().Where(o => !o.IsDeleted);

        if (pagination.SearchTerm is not null)
        {
            // Используем ILike для case-insensitive поиска в PostgreSQL (оптимизировано для индексов)
            query = query.Where(o =>
                EF.Functions.ILike(o.Name, $"%{pagination.SearchTerm}%") ||
                EF.Functions.ILike(o.Inn!, $"%{pagination.SearchTerm}%") ||
                EF.Functions.ILike(o.Address!, $"%{pagination.SearchTerm}%"));
        }

        return await query
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(o => o.ToReadDto())
            .ToListAsync(ct);
    }

    public async Task<ReadOrgDto?> FetchById(int id, CancellationToken ct)
    {
        return await context.Org
            .AsNoTracking()
            .Where(o => o.OrgId == id && !o.IsDeleted)
            .Select(o => o.ToReadDto())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> Update(int id, UpdateOrgDto dto, CancellationToken ct = default)
    {
        var existing = await context.Org.FindAsync([id], ct);
        if (existing is null or { IsDeleted: true }) return false;

        existing.Name = dto.Name ?? existing.Name;
        existing.Inn = dto.INN ?? existing.Inn;
        existing.Latitude = dto.Latitude ?? existing.Latitude;
        existing.Longitude = dto.Longitude ?? existing.Longitude;
        existing.Address = dto.Address ?? existing.Address;

        await context.SaveChangesAsync(ct);
        return true;
    }
}
