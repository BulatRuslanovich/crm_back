using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class PlanDAO(AppDBContext context) : IPlanDAO
{
    public async Task<ReadPlanDto?> Create(CreatePlanDto dto, CancellationToken ct = default)
    {
        if (dto.EndDate < dto.StartDate) throw new InvalidOperationException("End date cannot be earlier than start date");

        // Оптимизация: параллельная проверка существования User и Organization
        var userTask = context.User.AnyAsync(u => u.UsrId == dto.UsrId && !u.IsDeleted, ct);
        var orgTask = context.Org.AnyAsync(o => o.OrgId == dto.OrgId && !o.IsDeleted, ct);

        await Task.WhenAll(userTask, orgTask);

        if (!await userTask) throw new InvalidOperationException("User not found");
        if (!await orgTask) throw new InvalidOperationException("Organization not found");

        var entity = dto.ToEntity();
        context.Plan.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity.ToReadDto();
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.Plan.FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadPlanDto>> FetchAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = context.Plan
            .Include(p => p.User)
            .Include(p => p.Organization)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Используем ILike для case-insensitive поиска в PostgreSQL (оптимизировано для индексов)
            query = query.Where(p =>
                EF.Functions.ILike(p.User.FirstName!, $"%{searchTerm}%") ||
                EF.Functions.ILike(p.User.LastName!, $"%{searchTerm}%") ||
                EF.Functions.ILike(p.Organization.Name, $"%{searchTerm}%"));
        }

        var plans = await query
            .AsNoTracking()
            .OrderByDescending(p => p.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return plans.Select(p => p.ToReadDto()).ToList();
    }

    public async Task<ReadPlanDto?> FetchById(int id, CancellationToken ct)
    {
        var plan = await context.Plan
            .Include(p => p.User)
            .Include(p => p.Organization)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PlanId == id && !p.IsDeleted, ct);

        return plan?.ToReadDto();
    }

    public async Task<bool> Update(int id, UpdatePlanDto dto, CancellationToken ct = default)
    {
        var existing = await context.Plan.FindAsync([id], ct);
        if (existing is null or { IsDeleted: true }) return false;

        existing.StartDate = dto.StartDate ?? existing.StartDate;
        existing.EndDate = dto.EndDate ?? existing.EndDate;

        await context.SaveChangesAsync(ct);
        return true;
    }
}
