namespace CrmBack.Services.Impl;

using CrmBack.Core.Models.Dto;
using CrmBack.DAO;

public class ActivService(IActivDAO dao) : IActivService
{
    public async Task<ReadActivDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    public async Task<List<ReadActivDto>> GetAll(PaginationDto pagination, CancellationToken ct = default) =>
        await dao.FetchAll(pagination, ct);

    public async Task<ReadActivDto?> Create(CreateActivDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);

    public async Task<bool> Update(int id, UpdateActivDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);

    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);

    public async Task<List<ReadActivDto>> GetByUserId(int userId, CancellationToken ct = default) =>
        await dao.FetchByUserId(userId, ct);
}
