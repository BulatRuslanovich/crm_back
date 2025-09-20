namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload;

public interface IUserService
{
    public Task<ReadUserPayload> GetUserById(int id);

    public Task<IEnumerable<ReadUserPayload>> GetAllUsers();

    public Task<UserEntity> CreateUser(CreateUserPayload user);
}