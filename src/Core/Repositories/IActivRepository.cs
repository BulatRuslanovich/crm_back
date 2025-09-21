using System;
using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Repositories;

public interface IActivRepository
{
    public Task<ActivEntity?> GetByIdAsync(int id);
    public Task<IEnumerable<ActivEntity>> GetAllAsync(bool includeDeleted = false);
    public Task<int> CreateAsync(ActivEntity activ);
    public Task<bool> UpdateAsync(ActivEntity activ);
    public Task<bool> HardDeleteAsync(int id);
    public Task<bool> SoftDeleteAsync(int id);
}
