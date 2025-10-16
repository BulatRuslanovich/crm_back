namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Activ;

public interface IActivService
{
    public Task<ReadActivPayload?> GetActivById(int id);
    public Task<List<ReadActivPayload>> GetAllActiv(bool isDeleted, int page, int pageSize);
    public Task<ReadActivPayload?> CreateActiv(CreateActivPayload payload);
    public Task<bool> UpdateActiv(int id, UpdateActivPayload payload);
    public Task<bool> DeleteActiv(int id);
}