namespace CrmBack.Core.Repositories;

using CrmBack.Core.Models.Entities;

public interface IUserRepository : IRepository<int, UserEntity>
{
    public Task<UserEntity?> GetByLoginAsync(string login);
}
