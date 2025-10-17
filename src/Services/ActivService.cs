namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class ActivService(IActivRepository activRepository) : IActivService
{
    public async Task<ReadActivPayload?> GetActivById(int id, CancellationToken ct = default)
    {
        var activ = await activRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return activ?.ToReadPayload();
    }

    public async Task<List<ReadActivPayload>> GetAllActiv(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var actives = await activRepository.GetAllAsync(isDeleted, page, pageSize, ct).ConfigureAwait(false);

        return [.. actives.Select(u => u.ToReadPayload())];
    }

    public async Task<ReadActivPayload?> CreateActiv(CreateActivPayload payload, CancellationToken ct = default)
    {
        var activId = await activRepository.CreateAsync(payload.ToEntity(), ct).ConfigureAwait(false);
        var activDto = await activRepository.GetByIdAsync(activId, ct).ConfigureAwait(false);
        return activDto?.ToReadPayload();
    }

    public async Task<bool> UpdateActiv(int id, UpdateActivPayload payload, CancellationToken ct = default)
    {
        return await activRepository.UpdateAsync(payload.ToEntity(id), ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteActiv(int id, CancellationToken ct = default)
    {
        return await activRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);
    }
}