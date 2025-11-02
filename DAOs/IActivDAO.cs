using CrmBack.Core.Models.Dto;

namespace CrmBack.DAOs;

public interface IActivDAO : ICrudDAO<ReadActivDto, CreateActivDto, UpdateActivDto>
{
    public Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default);
}
