using CrmBack.Core.Extensions;
using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using CrmBack.Core.Specifications;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CrmBack.DAOs.Impl;

public class UserDAO(AppDBContext context, IDistributedCache cache) : BaseCrudDAO<UserEntity, ReadUserDto, CreateUserDto, UpdateUserDto>(context, cache), IUserDAO
{
    protected override string CacheKeyPrefix => "User";
    protected override ReadUserDto MapToDto(UserEntity entity) => entity.ToReadDto();

    protected override UserEntity MapToEntity(CreateUserDto dto) => dto.ToEntity();

    protected override void UpdateEntity(UserEntity entity, UpdateUserDto dto)
    {
        entity.FirstName = dto.FirstName ?? entity.FirstName;
        entity.LastName = dto.LastName ?? entity.LastName;
        entity.MiddleName = dto.MiddleName ?? entity.MiddleName;
        entity.Login = dto.Login ?? entity.Login;
    }

    protected override IQueryable<UserEntity> ApplyDefaults(IQueryable<UserEntity> query, PaginationDto pagination)
        => query
            .WhereNotDeleted()
            .AsNoTracking()
            .Search(pagination.SearchTerm)
            .OrderByDefault()
            .Paginate(pagination);

    public override async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct)
    {
        var existing = await Context.User
            .AsNoTracking()
            .Where(u => u.UsrId == id && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (existing is null) return false;

        if (!string.IsNullOrEmpty(dto.Password) && !string.IsNullOrEmpty(dto.CurrentPassword))
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, existing.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");
            existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        UpdateEntity(existing, dto);
        await Context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default)
    {
        var entities = await Context.Activ
            .WhereNotDeleted()
            .Include(a => a.Organization)
            .Include(a => a.Status)
            .Where(a => a.UsrId == userId)
            .OrderByDefault()
            .ToListAsync(ct);
        return entities.Select(a => a.ToHumReadDto()).ToList();
    }

    public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
    {
        var user = await Context.User
            .WhereNotDeleted()
            .AsNoTracking()
            .Include(u => u.UserPolicies)
            .ThenInclude(up => up.Policy)
            .Where(u => u.Login == dto.Login)
            .FirstOrDefaultAsync(ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName ?? string.Empty, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }

    public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
    {
        var user = await Context.User
            .WhereNotDeleted()
            .AsNoTracking()
            .Include(u => u.UserPolicies)
            .ThenInclude(up => up.Policy)
            .Where(u => u.UsrId == id)
            .FirstOrDefaultAsync(ct);

        if (user is null) return null;

        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }
}
