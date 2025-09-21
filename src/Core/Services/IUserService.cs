namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.User;

public interface IUserService
{
    public Task<ReadUserPayload> GetUserById(int id);

    public Task<IEnumerable<ReadUserPayload>> GetAllUsers();

    public Task<ReadUserPayload> CreateUser(CreateUserPayload payload);

    public Task<bool> UpdateUser(int id, UpdateUserPayload payload);

    public Task<bool> DeleteUser(int id); 
}