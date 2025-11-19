using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Specifications;
using CrmBack.Application.Users.Dto;
using CrmBack.Core.Extensions;
using CrmBack.Domain.Users;
using CrmBack.Infrastructure.Data;
using CrmBack.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Persistence.Users;


public class UserDAO(AppDBContext context) : BaseCrudDAO<UserEntity, ReadUserDto, CreateUserDto, UpdateUserDto>(context,
	e => e.ToReadDto(),
	d => d.ToEntity(),
	(e, d) => e.Update(d),
	(q, p) => q.WhereNotDeleted().AsNoTracking().Search(p.SearchTerm).OrderByDefault().Paginate(p)
), IUserDAO
{

	public override async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct)
	{
		var existing = await Context.User
			.Where(u => u.UsrId == id && !u.IsDeleted)
			.FirstOrDefaultAsync(ct);

		if (existing is null) return false;

		if (!string.IsNullOrEmpty(dto.Password) && !string.IsNullOrEmpty(dto.CurrentPassword))
		{
			if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, existing.PasswordHash))
				throw new UnauthorizedAccessException("Current password is incorrect");
			existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
		}

		existing.Update(dto);
		return await Context.SaveChangesAsync(ct) > 0;
	}


	public async Task<List<HumReadActivDto>> FetchHumActivs(int userId, CancellationToken ct = default)
	{
		var entities = await Context.Activ
			.WhereNotDeleted()
			.Include(a => a.Organization)
			.Include(a => a.Status)
			.Where(a => a.UsrId == userId)
			.OrderByDefault()
			.ToListAsync(ct);

		return entities.Select(a => a.ToHumReadDto()).ToList();
	}


	public async Task<UserWithPoliciesDto?> FetchByLogin(LoginUserDto dto, CancellationToken ct = default)
	{
		var user = await Context.User.Include(u => u.UserPolicies)
									 .ThenInclude(p => p.Policy)
									 .Where(u => u.Login == dto.Login && !u.IsDeleted)
									 .FirstOrDefaultAsync(ct);

		if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

		return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName ?? string.Empty, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
	}

	public async Task<UserWithPoliciesDto?> FetchByIdWithPolicies(int id, CancellationToken ct = default)
	{
		var user = await Context.User.Include(u => u.UserPolicies)
									 .ThenInclude(p => p.Policy)
									 .Where(u => u.UsrId == id && !u.IsDeleted)
									 .FirstOrDefaultAsync(ct);

		if (user is null) return null;

		return new UserWithPoliciesDto(user.UsrId, user.FirstName, user.LastName, user.MiddleName, user.Login, [.. user.UserPolicies.Select(up => up.Policy.ToReadDto())]);
	}
}
