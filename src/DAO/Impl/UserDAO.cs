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
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReadUserDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = context.User.AsQueryable();

        if (!isDeleted)
        {
            query.Where(o => !o.IsDeleted);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u =>
                u.FirstName!.Contains(searchTerm) ||
                u.LastName!.Contains(searchTerm) ||
                u.Login.Contains(searchTerm));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return [.. users.Select(u => u.ToReadDto())];
    }

    public async Task<ReadUserDto?> FetchById(int id, CancellationToken ct)
    {
        var user = await context.User
            .FirstOrDefaultAsync(u => u.UsrId == id && !u.IsDeleted, ct);

        return user?.ToReadDto();
    }

    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var existing = await context.User.FindAsync([id], ct);
        if (existing == null || existing.IsDeleted) return false;

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
        var activs = await context.Activ
            .Where(a => a.UsrId == userId && !a.IsDeleted)
            .Include(a => a.Organization)
            .Include(a => a.Status)
            .OrderByDescending(a => a.VisitDate)
            .ToListAsync(ct);

        return [.. activs.Select(a => a.ToHumReadDto())];
    }
}
