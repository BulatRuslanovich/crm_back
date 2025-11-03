using CrmBack.Application.Common.Dto;

namespace CrmBack.Infrastructure.Persistence.Common;

public interface ICrudDAO<ReadDto, CreateDto, UpdateDto>
{
    public Task<List<ReadDto>> FetchAll(PaginationDto pagination, CancellationToken ct = default);

    public Task<ReadDto?> FetchById(int id, CancellationToken ct);

    public Task<ReadDto?> Create(CreateDto dto, CancellationToken ct = default);

    public Task<bool> Update(int id, UpdateDto dto, CancellationToken ct = default);

    public Task<bool> Delete(int id, CancellationToken ct = default);
}
