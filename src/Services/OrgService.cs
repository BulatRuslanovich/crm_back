namespace CrmBack.Services;

using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Repositories;
using CrmBack.Core.Services;
using CrmBack.Core.Utils.Mapper;

public class OrgService(IOrgRepository orgRepository) : IOrgService
{
    public async Task<ReadOrgPayload?> GetOrgById(int id)
    {
        var org = await orgRepository.GetByIdAsync(id).ConfigureAwait(false);
        return org?.ToReadPayload();
    }

    public async Task<ReadOrgPayload?> CreateOrg(CreateOrgPayload payload)
    {
        var orgId = await orgRepository.CreateAsync(payload.ToEntity()).ConfigureAwait(false);
        var org = await orgRepository.GetByIdAsync(orgId).ConfigureAwait(false);
        return org?.ToReadPayload();
    }

    public async Task<bool> DeleteOrg(int id)
    {
        return await orgRepository.SoftDeleteAsync(id).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ReadOrgPayload>> GetAllOrgs()
    {
        var orgs = await orgRepository.GetAllAsync().ConfigureAwait(false);
        return orgs.ToReadPayloads();
    }

    public async Task<bool> UpdateOrg(int id, UpdateOrgPayload payload)
    {
        return await orgRepository.UpdateAsync(payload.ToEntity(id)).ConfigureAwait(false);
    }
}
