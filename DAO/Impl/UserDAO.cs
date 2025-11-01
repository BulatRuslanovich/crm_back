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

    // Пагинация и поиск пользователей по имени/фамилии/логину с сортировкой
    public async Task<List<ReadUserDto>> FetchAll(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = context.User.AsQueryable().Where(o => !o.IsDeleted);

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

        return users.Select(u => u.ToReadDto()).ToList();
    }

    public async Task<ReadUserDto?> FetchById(int id, CancellationToken ct)
    {
        var user = await context.User
            .FirstOrDefaultAsync(u => u.UsrId == id && !u.IsDeleted, ct);

        return user?.ToReadDto();
    }

    // Обновление данных пользователя с проверкой текущего пароля перед изменением
    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var existing = await context.User.FindAsync([id], ct);
        if (existing is null or { IsDeleted: true }) return false;

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

        return activs.Select(a => a.ToHumReadDto()).ToList();
    }

    // Поиск пользователя по логину с проверкой пароля и загрузкой связанных политик доступа
    public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
    {
        var user = await context.User.FirstOrDefaultAsync(u => u.Login == dto.Login && !u.IsDeleted, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        var usrPolicies = await context.UserPolicies
            .Where(up => up.UsrId == user.UsrId)
            .Include(up => up.Policy)
            .Select(up => up.Policy)
            .Where(p => !p.IsDeleted)
            .ToListAsync(ct);

        return new UserWithPoliciesDto
        {
            UsrId = user.UsrId,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            Policies = usrPolicies.Select(p => p.ToReadDto()).ToList()
        };
    }

    // Загрузка пользователя по ID вместе с его политиками доступа (ролями)
    public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
    {
        var user = await context.User.FirstOrDefaultAsync(u => u.UsrId == id && !u.IsDeleted, ct);
        if (user is null) return null;

        var usrPolicies = await context.UserPolicies
            .Where(up => up.UsrId == user.UsrId)
            .Include(up => up.Policy)
            .Select(up => up.Policy)
            .Where(p => !p.IsDeleted)
            .ToListAsync(ct);

        return new UserWithPoliciesDto
        {
            UsrId = user.UsrId,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            Policies = usrPolicies.Select(p => p.ToReadDto()).ToList()
        };
    }
}
