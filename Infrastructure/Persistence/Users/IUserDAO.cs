using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Users.Dto;
using CrmBack.Infrastructure.Persistence.Common;

namespace CrmBack.Infrastructure.Persistence.Users;

/// <summary>
/// User Data Access Object interface
/// Extends ICrudDAO with user-specific operations
/// </summary>
public interface IUserDAO : ICrudDAO<ReadUserDto, CreateUserDto, UpdateUserDto>
{
    /// <summary>Fetch user by login for authentication</summary>
    public Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default);

    /// <summary>Fetch user with policies (roles) by ID</summary>
    public Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default);

    /// <summary>Fetch human-readable activities for a user</summary>
    public Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default);
}
