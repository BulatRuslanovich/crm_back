namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class ActivService(IActivRepository activRepository) : IActivService
{
    public async Task<ReadActivPayload> GetActivById(int id)
    {
        var activ = await activRepository.GetByIdAsync(id).ConfigureAwait(false) ?? throw new NullReferenceException("User not exist");
        return activ.ToReadPayload();
    }

    public async Task<IEnumerable<ReadActivPayload>> GetAllActiv()
    {
        var actives = await activRepository.GetAllAsync().ConfigureAwait(false) ?? throw new NullReferenceException("User not exist");

        return actives.Select(u => u.ToReadPayload());
    }

    public async Task<ReadActivPayload> CreateActiv(CreateActivPayload payload)
    {
        var activId = await activRepository.CreateAsync(payload.ToEntity()).ConfigureAwait(false);
        var activDto = await activRepository.GetByIdAsync(activId).ConfigureAwait(false);
        return activDto?.ToReadPayload() ?? throw new InvalidOperationException("User was created but cannot be retrieved"); ;
    }

    public async Task<bool> UpdateActiv(int id, UpdateActivPayload payload)
    {
        return await activRepository.UpdateAsync(payload.ToEntity(id)).ConfigureAwait(false);
    }

    public async Task<bool> DeleteUser(int id)
    {
        return await activRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }
}
