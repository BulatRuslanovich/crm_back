namespace CrmBack.Services.Impl;

using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

public class ActivService(AppDBContext context) : IActivService
{
    public async Task<ReadActivDto?> GetById(int id, CancellationToken ct = default)
    {
        var res = await context.Activ
            .FirstOrDefaultAsync(a => a.ActivId == id && !a.IsDeleted, ct);
        return res?.ToReadDto();
    }

    public async Task<List<ReadActivDto>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = context.Activ.AsQueryable();

        if (!isDeleted)
        {
            query.Where(a => !a.IsDeleted);
        }

        var res = await query
            .OrderByDescending(a => a.CreatedAt) // Добавляем OrderBy перед Skip/Take
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return [.. res.Select(r => r.ToReadDto())];
    }

    public async Task<ReadActivDto?> Create(CreateActivDto Dto, CancellationToken ct = default)
    {
        var entity = Dto.ToEntity();
        context.Activ.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity.ToReadDto();
    }

    public async Task<bool> Update(int id, UpdateActivDto Dto, CancellationToken ct = default)
    {
        var existing = await context.Activ.FindAsync([id], ct);
        if (existing == null || existing.IsDeleted) return false;

        if (Dto.StatusId.HasValue)
            existing.StatusId = Dto.StatusId.Value;

        if (Dto.VisitDate.HasValue)
            existing.VisitDate = Dto.VisitDate.Value;

        if (Dto.StartTime.HasValue)
            existing.StartTime = Dto.StartTime.Value;

        if (Dto.EndTime.HasValue)
            existing.EndTime = Dto.EndTime.Value;

        if (Dto.Description != null)
            existing.Description = Dto.Description;

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

    public async Task<List<ReadActivDto>> GetByUserId(int userId, CancellationToken ct = default)
    {
        var res = await context.Activ
            .Where(a => a.UsrId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.VisitDate)
            .ToListAsync(ct);

        return [.. res.Select(a => a.ToReadDto())];
    }
}
