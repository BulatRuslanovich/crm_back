using CrmBack.Core.Models.Entities;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Core.Utils;

/// <summary>
/// Compiled queries for high-performance data access
/// </summary>
public static class CompiledQueries
{
    // Compiled query for fetching user by login with policies
    public static readonly Func<AppDBContext, string, Task<UserEntity?>> UserByLoginAsync =
        EF.CompileAsyncQuery((AppDBContext context, string login) =>
            context.User
                .Include(u => u.UserPolicies)
                .ThenInclude(up => up.Policy)
                .FirstOrDefault(u => u.Login == login && !u.IsDeleted));

    // Compiled query for fetching user by ID with policies
    public static readonly Func<AppDBContext, int, Task<UserEntity?>> UserByIdWithPoliciesAsync =
        EF.CompileAsyncQuery((AppDBContext context, int id) =>
            context.User
                .Include(u => u.UserPolicies)
                .ThenInclude(up => up.Policy)
                .FirstOrDefault(u => u.UsrId == id && !u.IsDeleted));
}

