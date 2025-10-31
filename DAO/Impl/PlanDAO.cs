using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class PlanDAO(AppDBContext context) : IPlanDAO
{
    public async Task<ReadPlanDto?> Create(CreatePlanDto dto, CancellationToken ct = default)
    {
        var userExists = await context.User.AnyAsync(u => u.UsrId == dto.UsrId && !u.IsDeleted, ct);
        if (!userExists) throw new InvalidOperationException("User not found");

        var orgExists = await context.Org.AnyAsync(o => o.OrgId == dto.OrgId && !o.IsDeleted, ct);
        if (!orgExists) throw new InvalidOperationException("Organization not found");

        if (dto.EndDate < dto.StartDate) throw new InvalidOperationException("End date cannot be earlier than start date");

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
        var query = context.Plan.AsQueryable().Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p =>
                p.User.FirstName!.Contains(searchTerm) ||
                p.User.LastName!.Contains(searchTerm) ||
                p.Organization.Name.Contains(searchTerm));
        }

        var plans = await query
            .OrderByDescending(p => p.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return plans.Select(p => p.ToReadDto()).ToList();
    }

    public async Task<ReadPlanDto?> FetchById(int id, CancellationToken ct)
    {
        var plan = await context.Plan
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
