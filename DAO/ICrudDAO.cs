using CrmBack.Core.Models.Dto;


namespace CrmBack.DAO;

public interface ICrudDAO<RDto, CDto, UDto>
{
    public Task<List<RDto>> FetchAll(PaginationDto pagination, CancellationToken ct = default);

    public Task<RDto?> FetchById(int id, CancellationToken ct);

    public Task<RDto?> Create(CDto dto, CancellationToken ct = default);

    public Task<bool> Update(int id, UDto dto, CancellationToken ct = default);

    public Task<bool> Delete(int id, CancellationToken ct = default);
}
