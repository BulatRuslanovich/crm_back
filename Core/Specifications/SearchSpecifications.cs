using CrmBack.Core.Models.Dto;
using CrmBack.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Core.Specifications;

/// <summary>
/// Search specifications for filtering entities by search terms
/// </summary>
public static class SearchSpecifications
{
    /// <summary>
    /// Searches users by first name, last name, or login
    /// </summary>
    public static IQueryable<UserEntity> Search(this IQueryable<UserEntity> queryable, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        return queryable.Where(u =>
            EF.Functions.ILike(u.FirstName, $"%{searchTerm}%") ||
            EF.Functions.ILike(u.LastName, $"%{searchTerm}%") ||
            EF.Functions.ILike(u.Login, $"%{searchTerm}%"));
    }

    /// <summary>
    /// Searches organizations by name, INN, or address
    /// </summary>
    public static IQueryable<OrgEntity> Search(this IQueryable<OrgEntity> queryable, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        return queryable.Where(o =>
            EF.Functions.ILike(o.Name, $"%{searchTerm}%") ||
            EF.Functions.ILike(o.Inn!, $"%{searchTerm}%") ||
            EF.Functions.ILike(o.Address, $"%{searchTerm}%"));
    }

    /// <summary>
    /// Searches activities by description
    /// </summary>
    public static IQueryable<ActivEntity> Search(this IQueryable<ActivEntity> queryable, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        return queryable.Where(a => EF.Functions.ILike(a.Description!, $"%{searchTerm}%"));
    }
}

