namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload;

public interface IUserService
{
    public Task<ReadUserPayload> GetUserById(int id);

    public Task<IEnumerable<ReadUserPayload>> GetAllUsers();

    public Task<ReadUserPayload> CreateUser(CreateUserPayload user);

    public Task<bool> UpdateUser(int id, UpdateUserPayload user);

    public Task<bool> DeleteUser(int id); 
}