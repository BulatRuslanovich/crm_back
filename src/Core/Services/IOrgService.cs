namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Org;
using System.Threading;

public interface IOrgService
{
    public Task<ReadOrgPayload?> GetOrgById(int id, CancellationToken ct = default);
    public Task<List<ReadOrgPayload>> GetAllOrgs(bool isDeleted, int page, int pageSize, CancellationToken ct = default);
    public Task<ReadOrgPayload?> CreateOrg(CreateOrgPayload payload, CancellationToken ct = default);
    public Task<bool> UpdateOrg(int id, UpdateOrgPayload payload, CancellationToken ct = default);
    public Task<bool> DeleteOrg(int id, CancellationToken ct = default);
}