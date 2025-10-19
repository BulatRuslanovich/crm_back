namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Status;

public interface IActivService : IService<ReadActivPayload, CreateActivPayload, UpdateActivPayload> {
    public Task<List<ReadStatusPayload>> GetAllStatus(CancellationToken ct = default);
}
