using CrmBack.Application.Common.Dto;
using CrmBack.Domain.Activities;
using CrmBack.Domain.Common;
using CrmBack.Domain.Organizations;
using CrmBack.Domain.Users;

namespace CrmBack.Core.Extensions;

public static class QueryableExtensions
{
	public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> queryable) where T : BaseEntity
		=> queryable.Where(e => !e.IsDeleted);

	public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDto pagination)
		=> queryable
			.Skip((pagination.Page - 1) * pagination.PageSize)
			.Take(pagination.PageSize);

	public static IQueryable<UserEntity> OrderByDefault(this IQueryable<UserEntity> queryable)
		=> queryable.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

	public static IQueryable<OrgEntity> OrderByDefault(this IQueryable<OrgEntity> queryable)
		=> queryable.OrderBy(o => o.Name);

	public static IQueryable<ActivEntity> OrderByDefault(this IQueryable<ActivEntity> queryable)
		=> queryable.OrderByDescending(a => a.VisitDate);
}

