using CrmBack.Application.Common.Dto;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Infrastructure.Persistence.Organizations;

namespace CrmBack.Application.Organizations.Services;


public class OrgService(IOrgDAO dao) : IOrgService
{
    public async Task<ReadOrgDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    public async Task<List<ReadOrgDto>> GetAll(PaginationDto pagination, CancellationToken ct = default) =>
        await dao.FetchAll(pagination, ct);

    public async Task<ReadOrgDto?> Create(CreateOrgDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);

    public async Task<bool> Update(int id, UpdateOrgDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);

    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);
}
