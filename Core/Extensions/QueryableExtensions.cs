using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Core.Extensions;

/// <summary>
/// Extension methods for IQueryable to add common query patterns
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Filters out soft-deleted entities
    /// </summary>
    public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> queryable) where T : BaseEntity
        => queryable.Where(e => !e.IsDeleted);

    /// <summary>
    /// Applies pagination to a query
    /// </summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDto pagination)
        => queryable
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize);

    /// <summary>
    /// Applies default ordering by primary key descending
    /// </summary>
    public static IQueryable<UserEntity> OrderByDefault(this IQueryable<UserEntity> queryable)
        => queryable.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

    /// <summary>
    /// Applies default ordering for organizations
    /// </summary>
    public static IQueryable<OrgEntity> OrderByDefault(this IQueryable<OrgEntity> queryable)
        => queryable.OrderBy(o => o.Name);

    /// <summary>
    /// Applies default ordering for activities
    /// </summary>
    public static IQueryable<ActivEntity> OrderByDefault(this IQueryable<ActivEntity> queryable)
        => queryable.OrderByDescending(a => a.VisitDate);
}

