using CrmBack.Application.Activities.Dto;
using CrmBack.Infrastructure.Persistence.Common;

namespace CrmBack.Infrastructure.Persistence.Activities;

/// <summary>
/// Activities Data Access Object interface
/// Extends ICrudDAO with activity-specific operations
/// </summary>
public interface IActivDAO : ICrudDAO<ReadActivDto, CreateActivDto, UpdateActivDto>
{
	/// <summary>Fetch all activities for a specific user</summary>
	public Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default);
}
