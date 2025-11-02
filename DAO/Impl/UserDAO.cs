using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.DAO.Impl;

public class UserDAO(AppDBContext context) : IUserDAO
{
    public async Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default)
    {
        var entity = dto.ToEntity();
        context.User.Add(entity);
        await context.SaveChangesAsync(ct);

        return entity.ToReadDto();
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.User.FindAsync([id], ct);
        if (entity is null or { IsDeleted: true }) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadUserDto>> FetchAll(PaginationDto pagination, CancellationToken ct = default)
    {
        var query = context.User.AsQueryable().Where(o => !o.IsDeleted);

        if (pagination.SearchTerm is not null)
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName!, $"%{pagination.SearchTerm}%") ||
                EF.Functions.ILike(u.LastName!, $"%{pagination.SearchTerm}%") ||
                EF.Functions.ILike(u.Login!, $"%{pagination.SearchTerm}%"));
        }

        return await query
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(u => u.ToReadDto())
            .ToListAsync(ct);   
    }

    public async Task<ReadUserDto?> FetchById(int id, CancellationToken ct)
    {
        return await context.User
            .AsNoTracking()
            .Where(u => u.UsrId == id && !u.IsDeleted)
            .Select(u => u.ToReadDto())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var existing = await context.User
            .AsNoTracking()
            .Where(u => u.UsrId == id && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);
            
        if (existing is null) return false;

        if (!string.IsNullOrEmpty(dto.Password) && !string.IsNullOrEmpty(dto.CurrentPassword))
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, existing.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");
            else
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        existing.FirstName = dto.FirstName ?? existing.FirstName;
        existing.LastName = dto.LastName ?? existing.LastName;
        existing.MiddleName = dto.MiddleName ?? existing.MiddleName;
        existing.Login = dto.Login ?? existing.Login;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default)
    {
        return await context.Activ
            .Where(a => a.UsrId == userId && !a.IsDeleted)
            .Include(a => a.Organization)
            .Include(a => a.Status)
            .OrderByDescending(a => a.VisitDate)
            .Select(a => a.ToHumReadDto())
            .ToListAsync(ct);
    }

    public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
    {
        var user = await context.User
            .AsNoTracking()
            .Include(u => u.UserPolicies)
            .ThenInclude(up => up.Policy)
            .Where(u => u.Login == dto.Login && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        return new UserWithPoliciesDto(user.UsrId, user.Login, user.FirstName, user.LastName, user.MiddleName ?? string.Empty, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]); 
    }

    public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
    {
        var user = await context.User.AsNoTracking()
            .Include(u => u.UserPolicies)
            .ThenInclude(up => up.Policy)
            .Where(u => u.UsrId == id && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (user is null) return null;

        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }
}
    