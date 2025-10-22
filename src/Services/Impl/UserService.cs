using BCrypt.Net;
using CrmBack.Core.Models.Dto;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;


namespace CrmBack.Services.Impl;
public class UserService(AppDBContext context) : IUserService
{
    public async Task<ReadUserDto?> GetById(int id, CancellationToken ct = default)
    {
        var user = await context.User
            .FirstOrDefaultAsync(u => u.UsrId == id && !u.IsDeleted, ct);

        return user?.ToReadDto();
    }

    public async Task<List<ReadUserDto>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
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

    public async Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default)
    {
        var entity = dto.ToEntity();
        context.User.Add(entity);
        await context.SaveChangesAsync(ct);

        return entity.ToReadDto();
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

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        var entity = await context.User.FindAsync([id], ct);
        if (entity == null || entity.IsDeleted) return false;

        entity.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<LoginResponseDto> Login(LoginUserDto Dto, CancellationToken ct = default)
    {
        var user = await context.User
            .FirstOrDefaultAsync(u => u.Login == Dto.Login && !u.IsDeleted, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid login or password");

        // return new LoginResponseDto(
        //     UserId: user.UsrId,
        //     Token: "dummy_token", // Заменить на реальную генерацию JWT
        //     RefreshToken: "dummy_refresh_token" // Заменить на реальную генерацию
        // );

        return new LoginResponseDto();
    }

    public async Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default)
    {
        var activs = await context.Activ
            .Where(a => a.UsrId == userId && !a.IsDeleted)
            .Include(a => a.Organization)
            .Include(a => a.Status)
            .OrderByDescending(a => a.VisitDate)
            .ToListAsync(ct);

        return [.. activs.Select(a => a.ToHumReadDto())];
    }

    public async Task<List<ReadPlanDto>> GetPlans(int userId, CancellationToken ct = default)
    {
        var plans = await context.Plan
            .Where(p => p.UsrId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(ct);

        return [.. plans.Select(p => p.ToReadDto())];
    }
}
