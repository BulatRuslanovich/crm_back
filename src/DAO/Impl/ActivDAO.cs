using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class ActivDAO(AppDBContext context) : IActivDAO
{
    public async Task<List<ReadActivDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = context.Activ.AsQueryable();

        if (!isDeleted)
        {
            query.Where(a => !a.IsDeleted);
        }

        var res = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return [.. res.Select(r => r.ToReadDto())];
    }

    public async Task<ReadActivDto?> FetchById(int id, CancellationToken ct)
    {
        var res = await context.Activ
            .FirstOrDefaultAsync(a => a.ActivId == id && !a.IsDeleted, ct);
        return res?.ToReadDto();
    }

    public async Task<ReadActivDto?> Create(CreateActivDto dto, CancellationToken ct = default)
    {
        var entity = dto.ToEntity();
        context.Activ.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity.ToReadDto();
    }

    public async Task<bool> Update(int id, UpdateActivDto dto, CancellationToken ct = default)
    {
        var existing = await context.Activ.FindAsync([id], ct);
        if (existing == null || existing.IsDeleted) return false;

        if (dto.StatusId.HasValue)
            existing.StatusId = dto.StatusId.Value;

        if (dto.VisitDate.HasValue)
            existing.VisitDate = dto.VisitDate.Value;

        if (dto.StartTime.HasValue)
            existing.StartTime = dto.StartTime.Value;

        if (dto.EndTime.HasValue)
            existing.EndTime = dto.EndTime.Value;

        if (dto.Description != null)
            existing.Description = dto.Description;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.Activ.FindAsync([id], ct);
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default)
    {
        var res = await context.Activ
            .Where(a => a.UsrId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.VisitDate)
            .ToListAsync(ct);

        return [.. res.Select(a => a.ToReadDto())];
    }
}
