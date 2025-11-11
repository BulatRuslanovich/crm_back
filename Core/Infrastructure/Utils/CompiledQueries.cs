using CrmBack.Domain.Users;
using CrmBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Core.Infrastructure.Utils;

public static class CompiledQueries
{
	public static readonly Func<AppDBContext, string, Task<UserEntity?>> UserByLoginAsync =
		EF.CompileAsyncQuery((AppDBContext context, string login) =>
			context.User
				.Include(u => u.UserPolicies)
				.ThenInclude(up => up.Policy)
				.FirstOrDefault(u => u.Login == login && !u.IsDeleted));

	public static readonly Func<AppDBContext, int, Task<UserEntity?>> UserByIdWithPoliciesAsync =
		EF.CompileAsyncQuery((AppDBContext context, int id) =>
			context.User
				.Include(u => u.UserPolicies)
				.ThenInclude(up => up.Policy)
				.FirstOrDefault(u => u.UsrId == id && !u.IsDeleted));
}

