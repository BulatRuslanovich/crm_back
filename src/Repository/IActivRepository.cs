namespace CrmBack.Repository;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;

public interface IActivRepository : IRepository<int, ActivEntity>
{
    public Task<IEnumerable<HumReadActivPayload>> GetAllHumActivsByUserIdAsync(int userId, CancellationToken ct = default);

    public Task<IEnumerable<StatusEntity>> GetAllStatusAsync(CancellationToken ct = default);
}
