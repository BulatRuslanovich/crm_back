namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.User;
using System.Threading;

public interface IUserService
{
    public Task<ReadUserPayload?> GetUserById(int id, CancellationToken ct = default);

    public Task<List<ReadUserPayload>> GetAllUsers(bool isDeleted, int page, int pageSize, CancellationToken ct = default);

    public Task<ReadUserPayload?> CreateUser(CreateUserPayload payload, CancellationToken ct = default);

    public Task<bool> UpdateUser(int id, UpdateUserPayload payload, CancellationToken ct = default);

    public Task<bool> DeleteUser(int id, CancellationToken ct = default);

    public Task<LoginResponsePayload> LoginUser(LoginUserPayload payload, CancellationToken ct = default);
}