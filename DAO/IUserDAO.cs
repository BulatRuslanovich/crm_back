using CrmBack.Core.Models.Dto;


namespace CrmBack.DAO;

public interface IUserDAO
{
    public Task<List<ReadUserDto>> FetchAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);

    public Task<ReadUserDto?> FetchById(int id, CancellationToken ct);

    public Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default);

    public Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default);

    public Task<bool> Delete(int id, CancellationToken ct = default);

    public Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default);

    public Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default);

    public Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default);
}
