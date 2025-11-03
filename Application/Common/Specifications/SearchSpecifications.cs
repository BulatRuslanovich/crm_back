using CrmBack.Domain.Users;
using CrmBack.Domain.Organizations;
using CrmBack.Domain.Activities;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Application.Common.Specifications;

public static class SearchSpecifications
{
    public static IQueryable<UserEntity> Search(this IQueryable<UserEntity> queryable, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        return queryable.Where(u =>
            EF.Functions.ILike(u.FirstName, $"%{searchTerm}%") ||
            EF.Functions.ILike(u.LastName, $"%{searchTerm}%") ||
            EF.Functions.ILike(u.Login, $"%{searchTerm}%"));
    }

    public static IQueryable<OrgEntity> Search(this IQueryable<OrgEntity> queryable, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        return queryable.Where(o =>
            EF.Functions.ILike(o.Name, $"%{searchTerm}%") ||
            EF.Functions.ILike(o.Inn!, $"%{searchTerm}%") ||
            EF.Functions.ILike(o.Address, $"%{searchTerm}%"));
    }

    public static IQueryable<ActivEntity> Search(this IQueryable<ActivEntity> queryable, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        return queryable.Where(a => EF.Functions.ILike(a.Description!, $"%{searchTerm}%"));
    }
}

