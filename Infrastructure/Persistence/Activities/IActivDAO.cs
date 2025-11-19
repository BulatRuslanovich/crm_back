using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Application.Activities.Dto;
using CrmBack.Infrastructure.Persistence.Common;

namespace CrmBack.Infrastructure.Persistence.Activities;


public interface IActivDAO : ICrudDAO<ReadActivDto, CreateActivDto, UpdateActivDto>
{
	public Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default);
}
