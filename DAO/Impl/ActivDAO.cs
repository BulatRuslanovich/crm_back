using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class ActivDAO(AppDBContext context) : IActivDAO
{
    public async Task<List<ReadActivDto>> FetchAll(PaginationDto pagination, CancellationToken ct = default)
    {
        var query = context.Activ.AsQueryable().Where(a => !a.IsDeleted);

        if (pagination.SearchTerm is not null)
        {
            query = query.Where(a =>
                EF.Functions.ILike(a.Description!, $"%{pagination.SearchTerm}%"));
        }

        return await query
            .AsNoTracking()
            .OrderByDescending(a => a.VisitDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
        .Select(a => a.ToReadDto())
        .ToListAsync(ct);
    }

    public async Task<ReadActivDto?> FetchById(int id, CancellationToken ct)
    {
        return await context.Activ
            .AsNoTracking()
            .Where(a => a.ActivId == id && !a.IsDeleted)
            .Select(a => a.ToReadDto())
            .FirstOrDefaultAsync(ct);
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
        if (existing is null or { IsDeleted: true }) return false;

        existing.StatusId = dto.StatusId ?? existing.StatusId;
        existing.VisitDate = dto.VisitDate ?? existing.VisitDate;
        existing.StartTime = dto.StartTime ?? existing.StartTime;
        existing.EndTime = dto.EndTime ?? existing.EndTime;
        existing.Description = dto.Description ?? existing.Description;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.Activ.FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default)
    {
        return await context.Activ
            .AsNoTracking()
            .Where(a => a.UsrId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.VisitDate)
            .Select(a => a.ToReadDto())
            .ToListAsync(ct);
    }
}
