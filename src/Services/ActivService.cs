namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class ActivService(IActivRepository activRepository) : IActivService
{
    public async Task<ReadActivPayload?> GetActivById(int id)
    {
        var activ = await activRepository.GetByIdAsync(id).ConfigureAwait(false);
        return activ?.ToReadPayload();
    }

    public async Task<List<ReadActivPayload>> GetAllActiv()
    {
        var actives = await activRepository.GetAllAsync().ConfigureAwait(false);

        return [.. actives.Select(u => u.ToReadPayload())];
    }

    public async Task<ReadActivPayload?> CreateActiv(CreateActivPayload payload)
    {
        var activId = await activRepository.CreateAsync(payload.ToEntity()).ConfigureAwait(false);
        var activDto = await activRepository.GetByIdAsync(activId).ConfigureAwait(false);
        return activDto?.ToReadPayload();
    }

    public async Task<bool> UpdateActiv(int id, UpdateActivPayload payload)
    {
        return await activRepository.UpdateAsync(payload.ToEntity(id)).ConfigureAwait(false);
    }

    public async Task<bool> DeleteActiv(int id)
    {
        return await activRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }
}
