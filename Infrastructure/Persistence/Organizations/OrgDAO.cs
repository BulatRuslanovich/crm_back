using System.Linq;
using CrmBack.Application.Common.Specifications;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Domain.Organizations;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Persistence.Organizations;

public class OrgDAO(AppDBContext context) : BaseCrudDAO<OrgEntity, ReadOrgDto, CreateOrgDto, UpdateOrgDto>(context,
	e => e.ToReadDto(),
	d => d.ToEntity(),
	(e, d) => e.Update(d),
	(q, p) => q.Where(e => !e.IsDeleted).AsNoTracking().Search(p.SearchTerm).OrderBy(o => o.Name).Skip((p.Page - 1) * p.PageSize)
			.Take(p.PageSize)
), IOrgDAO
{ }
