using CrmBack.Core.Extensions;
using CrmBack.Application.Common.Dto;
using CrmBack.Domain.Users;
using CrmBack.Application.Common.Specifications;
using CrmBack.Core.Infrastructure.Utils;
using CrmBack.Infrastructure.Data;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using CrmBack.Infrastructure.Persistence.Common;
using CrmBack.Application.Users.Dto;
using CrmBack.Application.Activities.Dto;

namespace CrmBack.Infrastructure.Persistence.Users;

public class UserDAO(AppDBContext context, IConnectionMultiplexer redis) : BaseCrudDAO<UserEntity, ReadUserDto, CreateUserDto, UpdateUserDto>(context, redis), IUserDAO
{
    protected override string CacheKeyPrefix => "User";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);
    private static readonly MessagePackSerializerOptions MessagePackOptions = MessagePackSerializerOptions.Standard
        .WithResolver(ContractlessStandardResolver.Instance)
        .WithCompression(MessagePackCompression.Lz4Block);
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
        string cacheKey = $"CrmBack:UserActivs:{userId}";
        var db = Redis.GetDatabase();
        var cached = await db.StringGetAsync(cacheKey);

        if (cached.HasValue)
            return MessagePackSerializer.Deserialize<List<HumReadActivDto>>(cached!, MessagePackOptions, ct) ?? [];

        var entities = await Context.Activ
            .WhereNotDeleted()
            .Include(a => a.Organization)
            .Include(a => a.Status)
            .Where(a => a.UsrId == userId)
            .OrderByDefault()
            .ToListAsync(ct);

        var result = entities.Select(a => a.ToHumReadDto()).ToList();
        var serialized = MessagePackSerializer.Serialize(result, MessagePackOptions, ct);
        await db.StringSetAsync(cacheKey, serialized, CacheExpiration);

        return result;
    }

    public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
    {
        ct = default;
        var user = await CompiledQueries.UserByLoginAsync(Context, dto.Login);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName ?? string.Empty, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }

    public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
    {
        ct = default;
        var user = await CompiledQueries.UserByIdWithPoliciesAsync(Context, id);

        if (user is null) return null;

        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }
}
