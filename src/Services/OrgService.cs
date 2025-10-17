namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class OrgService(IOrgRepository orgRepository) : IOrgService
{
    public async Task<ReadOrgPayload?> GetOrgById(int id, CancellationToken ct = default)
    {
        var org = await orgRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return org?.ToReadPayload();
    }

    public async Task<ReadOrgPayload?> CreateOrg(CreateOrgPayload payload, CancellationToken ct = default)
    {
        var orgId = await orgRepository.CreateAsync(payload.ToEntity(), ct).ConfigureAwait(false);
        var org = await orgRepository.GetByIdAsync(orgId, ct).ConfigureAwait(false);
        return org?.ToReadPayload();
    }

    public async Task<bool> DeleteOrg(int id, CancellationToken ct = default)
    {
        return await orgRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);
    }

    public async Task<List<ReadOrgPayload>> GetAllOrgs(bool isDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var orgs = await orgRepository.GetAllAsync(isDeleted, page, pageSize, ct).ConfigureAwait(false);
        return orgs.ToReadPayloads();
    }

    public async Task<bool> UpdateOrg(int id, UpdateOrgPayload payload, CancellationToken ct = default)
    {
        return await orgRepository.UpdateAsync(payload.ToEntity(id), ct).ConfigureAwait(false);
    }
}