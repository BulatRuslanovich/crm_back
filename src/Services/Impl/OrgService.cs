namespace CrmBack.Services.Impl;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Utils.Mapper;
using CrmBack.Repository;

public class OrgService(IOrgRepository orgRepository) : IOrgService
{
    public async Task<ReadOrgPayload?> GetById(int id, CancellationToken ct = default)
    {
        var org = await orgRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        return org?.ToReadPayload();
    }

    public async Task<ReadOrgPayload?> Create(CreateOrgPayload payload, CancellationToken ct = default)
    {
        var orgId = await orgRepository.CreateAsync(payload.ToEntity(), ct).ConfigureAwait(false);
        var org = await orgRepository.GetByIdAsync(orgId, ct).ConfigureAwait(false);
        return org?.ToReadPayload();
    }

    public async Task<bool> Delete(int id, CancellationToken ct = default)
    {
        return await orgRepository.SoftDeleteAsync(id, ct).ConfigureAwait(false);
    }

    public async Task<List<ReadOrgPayload>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        if (searchTerm != null) {
            

            var findedOrgs = await orgRepository.FindByAsync("name", searchTerm, exactMatch: false, ct: ct);
            return [.. findedOrgs.Select(o => o.ToReadPayload())];
        }

        var orgs = await orgRepository.GetAllAsync(isDeleted, page, pageSize, ct).ConfigureAwait(false);
        return [.. orgs.Select(o => o.ToReadPayload())];
    }

    public async Task<bool> Update(int id, UpdateOrgPayload payload, CancellationToken ct = default)
    {
        var existing = await orgRepository.GetByIdAsync(id, ct).ConfigureAwait(false);

        if (existing == null) return false;

        var newEntity = new OrgEntity(
            org_id: id,
            name: payload.Name ?? existing.name,
            inn: payload.INN ?? existing.inn,
            latitude: payload.Latitude ?? existing.latitude,
            longitude: payload.Longitude ?? existing.longitude,
            address: payload.Address ?? existing.address,
            is_deleted: existing.is_deleted
        );

        return await orgRepository.UpdateAsync(newEntity, ct).ConfigureAwait(false);
    }
}
