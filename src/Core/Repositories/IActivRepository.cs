namespace CrmBack.Core.Repositories;

using CrmBack.Core.Models.Entities;

public interface IActivRepository
{
    public Task<ActivEntity?> GetByIdAsync(int id);
    public Task<IEnumerable<ActivEntity>> GetAllAsync(bool includeDeleted = false);
    public Task<int> CreateAsync(ActivEntity activ);
    public Task<bool> UpdateAsync(ActivEntity activ);
    public Task<bool> SoftDeleteAsync(int id);
    public Task<bool> HardDeleteAsync(int id);
}
