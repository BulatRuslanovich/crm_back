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
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadOrgDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = context.Org.AsQueryable();

        if (!isDeleted)
        {
            query.Where(o => !o.IsDeleted);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o =>
                o.Name.Contains(searchTerm) ||
                o.Inn!.Contains(searchTerm) ||
                o.Address!.Contains(searchTerm));
        }

        var orgs = await query
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return [.. orgs.Select(o => o.ToReadDto())];
    }

    public async Task<ReadOrgDto?> FetchById(int id, CancellationToken ct)
    {
        var org = await context.Org
            .FirstOrDefaultAsync(o => o.OrgId == id && !o.IsDeleted, ct);

        return org?.ToReadDto();
    }

    public async Task<bool> Update(int id, UpdateOrgDto dto, CancellationToken ct = default)
    {
        var existing = await context.Org.FindAsync([id], ct);
        if (existing == null || existing.IsDeleted) return false;

        if (!string.IsNullOrEmpty(dto.Name))
            existing.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.INN))
            existing.Inn = dto.INN;

        if (dto.Latitude.HasValue)
            existing.Latitude = dto.Latitude.Value;

        if (dto.Longitude.HasValue)
            existing.Longitude = dto.Longitude.Value;

        if (!string.IsNullOrEmpty(dto.Address))
            existing.Address = dto.Address;

        await context.SaveChangesAsync(ct);
        return true;
    }
}
