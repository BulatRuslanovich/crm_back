namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.User;

public interface IUserService
{
    public Task<ReadUserPayload?> GetUserById(int id);

    public Task<List<ReadUserPayload>> GetAllUsers(bool isDeleted, int page, int pageSize);

    public Task<ReadUserPayload?> CreateUser(CreateUserPayload payload);

    public Task<bool> UpdateUser(int id, UpdateUserPayload payload);

    public Task<bool> DeleteUser(int id);

    public Task<LoginResponsePayload> LoginUser(LoginUserPayload payload);
}