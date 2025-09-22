namespace CrmBack.Core.Repositories;

using CrmBack.Core.Models.Entities;

public interface IUserRepository
{
    public Task<UserEntity?> GetByIdAsync(int id);
    public Task<IEnumerable<UserEntity>> GetAllAsync(bool includeDeleted = false, int page = 1, int pageSize = 10);
    public Task<UserEntity?> GetByLoginAsync(string login);
    public Task<int> CreateAsync(UserEntity user);
    public Task<bool> UpdateAsync(UserEntity user);
    public Task<bool> SoftDeleteAsync(int id);
    public Task<bool> HardDeleteAsync(int id);
}
