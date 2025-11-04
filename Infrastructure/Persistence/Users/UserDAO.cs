using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Dto;
using CrmBack.Application.Common.Specifications;
using CrmBack.Application.Users.Dto;
using CrmBack.Core.Extensions;
using CrmBack.Core.Infrastructure.Utils;
using CrmBack.Domain.Users;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Common;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CrmBack.Infrastructure.Persistence.Users;

/// <summary>
/// User Data Access Object implementation
/// Provides database operations for User entities with caching and performance optimizations
/// </summary>
public class UserDAO(AppDBContext context, IConnectionMultiplexer redis) : BaseCrudDAO<UserEntity, ReadUserDto, CreateUserDto, UpdateUserDto>(context, redis), IUserDAO
{
    protected override string CacheKeyPrefix => "User";
    
    /// <summary>
    /// Extended cache expiration for user-specific queries: 10 minutes
    /// Longer TTL because user data changes less frequently
    /// </summary>
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// MessagePack options for user-specific caching (same as base class)
    /// </summary>
    private static readonly MessagePackSerializerOptions MessagePackOptions = MessagePackSerializerOptions.Standard
        .WithResolver(ContractlessStandardResolver.Instance)
        .WithCompression(MessagePackCompression.Lz4Block);
    
    protected override ReadUserDto MapToDto(UserEntity entity) => entity.ToReadDto();

    protected override UserEntity MapToEntity(CreateUserDto dto) => dto.ToEntity();

    /// <summary>
    /// Update entity properties (partial update - only provided fields)
    /// Null values are ignored (preserves existing values)
    /// </summary>
    protected override void UpdateEntity(UserEntity entity, UpdateUserDto dto)
    {
        entity.FirstName = dto.FirstName ?? entity.FirstName;
        entity.LastName = dto.LastName ?? entity.LastName;
        entity.MiddleName = dto.MiddleName ?? entity.MiddleName;
        entity.Login = dto.Login ?? entity.Login;
    }

    /// <summary>
    /// Apply default query filters and pagination
    /// Performance: AsNoTracking() - reduces memory overhead (entities are read-only)
    /// </summary>
    protected override IQueryable<UserEntity> ApplyDefaults(IQueryable<UserEntity> query, PaginationDto pagination)
        => query
            .WhereNotDeleted()           // Filter soft-deleted entities
            .AsNoTracking()             // Performance: Don't track entities (read-only)
            .Search(pagination.SearchTerm)  // Apply search filter
            .OrderByDefault()           // Apply default sorting
            .Paginate(pagination);      // Apply pagination

    /// <summary>
    /// Override Update to handle password change with verification
    /// Security: Requires current password verification before changing password
    /// </summary>
    public override async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct)
    {
        // Use AsNoTracking for read-only query (performance optimization)
        var existing = await Context.User
            .AsNoTracking()
            .Where(u => u.UsrId == id && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (existing is null) return false;

        // Password change requires current password verification
        if (!string.IsNullOrEmpty(dto.Password) && !string.IsNullOrEmpty(dto.CurrentPassword))
        {
            // Verify current password before allowing change
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, existing.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");
            // Hash new password with BCrypt
            existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        UpdateEntity(existing, dto);
        await Context.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>
    /// Fetch user activities with caching
    /// Performance: Redis cache with 10-minute expiration
    /// Includes related Organization and Status entities for human-readable format
    /// </summary>
    public async Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default)
    {
        string cacheKey = $"CrmBack:UserActivs:{userId}";
        var db = Redis.GetDatabase();
        var cached = await db.StringGetAsync(cacheKey);

        // Cache hit: Return cached activities
        if (cached.HasValue)
            return MessagePackSerializer.Deserialize<List<HumReadActivDto>>(cached!, MessagePackOptions, ct) ?? [];

        // Cache miss: Query database with includes for related entities
        var entities = await Context.Activ
            .WhereNotDeleted()
            .Include(a => a.Organization)  // Eager load Organization
            .Include(a => a.Status)        // Eager load Status
            .Where(a => a.UsrId == userId)
            .OrderByDefault()
            .ToListAsync(ct);

        var result = entities.Select(a => a.ToHumReadDto()).ToList();
        // Cache the results
        var serialized = MessagePackSerializer.Serialize(result, MessagePackOptions, ct);
        await db.StringSetAsync(cacheKey, serialized, CacheExpiration);

        return result;
    }

    /// <summary>
    /// Fetch user by login for authentication
    /// Performance: Uses compiled query (pre-compiled LINQ expression) for better performance
    /// Security: Verifies password hash using BCrypt
    /// </summary>
    public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
    {
        ct = default;
        // Compiled query: Pre-compiled LINQ expression (faster than regular query)
        var user = await CompiledQueries.UserByLoginAsync(Context, dto.Login);

        // Verify password hash
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        // Return user with policies (roles) loaded
        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName ?? string.Empty, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }

    /// <summary>
    /// Fetch user with policies (roles) by ID
    /// Performance: Uses compiled query for better performance
    /// Used for token refresh operations (needs user roles)
    /// </summary>
    public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
    {
        ct = default;
        // Compiled query: Pre-compiled LINQ expression (faster than regular query)
        var user = await CompiledQueries.UserByIdWithPoliciesAsync(Context, id);

        if (user is null) return null;

        // Return user with policies (roles) loaded
        return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
    }
}
