using CrmBack.Application.Users.Dto;
using CrmBack.Application.Activities.Dto;
using CrmBack.Infrastructure.Persistence.Common;

namespace CrmBack.Infrastructure.Persistence.Users;

public interface IUserDAO : ICrudDAO<ReadUserDto, CreateUserDto, UpdateUserDto>
{

    public Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default);

    public Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default);

    public Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default);
}
