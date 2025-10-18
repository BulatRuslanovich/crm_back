namespace CrmBack.Repository;

using CrmBack.Core.Models.Entities;

public interface IUserRepository : IRepository<int, UserEntity>
{
    public Task<UserEntity?> GetByLoginAsync(string login, CancellationToken ct = default);
}
