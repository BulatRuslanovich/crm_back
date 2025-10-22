namespace CrmBack.Services.Impl;

using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

public class OrgService(AppDBContext context) : IOrgService
{
    public async Task<ReadOrgDto?> GetById(int id, CancellationToken ct = default)
    {
        var org = await context.Org
            .FirstOrDefaultAsync(o => o.OrgId == id && !o.IsDeleted, ct);

        return org?.ToReadDto();
    }

    public async Task<List<ReadOrgDto>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
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

    public async Task<ReadOrgDto?> Create(CreateOrgDto Dto, CancellationToken ct = default)
    {
        var entity = Dto.ToEntity();
        context.Org.Add(entity);
        await context.SaveChangesAsync(ct);

        return entity.ToReadDto();
    }

    public async Task<bool> Update(int id, UpdateOrgDto Dto, CancellationToken ct = default)
    {
        var existing = await context.Org.FindAsync([id], ct);
        if (existing == null || existing.IsDeleted) return false;

        if (!string.IsNullOrEmpty(Dto.Name))
            existing.Name = Dto.Name;

        if (!string.IsNullOrEmpty(Dto.INN))
            existing.Inn = Dto.INN;

        if (Dto.Latitude.HasValue)
            existing.Latitude = Dto.Latitude.Value;

        if (Dto.Longitude.HasValue)
            existing.Longitude = Dto.Longitude.Value;

        if (!string.IsNullOrEmpty(Dto.Address))
            existing.Address = Dto.Address;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.Org.FindAsync([id], ct);
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }
}
