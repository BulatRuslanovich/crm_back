using CrmBack.Core.Models.Dto;

namespace CrmBack.Services;
public interface IService<ReadDto, CreateDto, UpdateDto>
{
    public Task<ReadDto?> GetById(int id, CancellationToken ct = default);
    public Task<List<ReadDto>> GetAll(PaginationDto pagination, CancellationToken ct = default);
    public Task<ReadDto?> Create(CreateDto payload, CancellationToken ct = default);
    public Task<bool> Update(int id, UpdateDto payload, CancellationToken ct = default);
    public Task<bool> Delete(int id, CancellationToken ct = default);
}
