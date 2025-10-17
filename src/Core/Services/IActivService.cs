namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Activ;
using System.Threading;

public interface IActivService
{
    public Task<ReadActivPayload?> GetActivById(int id, CancellationToken ct = default);
    public Task<List<ReadActivPayload>> GetAllActiv(bool isDeleted, int page, int pageSize, CancellationToken ct = default);
    public Task<ReadActivPayload?> CreateActiv(CreateActivPayload payload, CancellationToken ct = default);
    public Task<bool> UpdateActiv(int id, UpdateActivPayload payload, CancellationToken ct = default);
    public Task<bool> DeleteActiv(int id, CancellationToken ct = default);
}