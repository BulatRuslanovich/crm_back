using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Specifications;
using CrmBack.Core.Extensions;
using CrmBack.Domain.Activities;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Persistence.Activities;


public class ActivDAO(AppDBContext context) : BaseCrudDAO<ActivEntity, ReadActivDto, CreateActivDto, UpdateActivDto>(
	context,
	e => e.ToReadDto(),
	d => d.ToEntity(),
	(e, d) => e.Update(d),
	(q, p) => q.WhereNotDeleted().AsNoTracking().Search(p.SearchTerm).OrderByDefault().Paginate(p)
), IActivDAO
{
	public async Task<List<ReadActivDto>> FetchByUserId(int userId, CancellationToken ct = default)
	{
		var entities = await Context.Activ
			.WhereNotDeleted()
			.AsNoTracking()
			.Where(a => a.UsrId == userId)
			.OrderByDefault()
			.ToListAsync(ct);
		return entities.Select(e => e.ToReadDto()).ToList();
	}
}
