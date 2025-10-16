namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Org;

public interface IOrgService
{
    public Task<ReadOrgPayload?> GetOrgById(int id);
    public Task<List<ReadOrgPayload>> GetAllOrgs(bool isDeleted, int page, int pageSize);
    public Task<ReadOrgPayload?> CreateOrg(CreateOrgPayload payload);
    public Task<bool> UpdateOrg(int id, UpdateOrgPayload payload);
    public Task<bool> DeleteOrg(int id);
}