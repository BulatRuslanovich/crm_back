using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmBack.Application.Common.Dto;
using CrmBack.Domain.Common;
using CrmBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmBack.Infrastructure.Persistence.Common;


public abstract class BaseCrudDAO<TEntity, ReadDto, CreateDto, UpdateDto>(AppDBContext context,
 Func<TEntity, ReadDto> mapToDto,
 Func<CreateDto, TEntity> mapToEntity,
 Action<TEntity, UpdateDto> updateEntity,
 Func<IQueryable<TEntity>, PaginationDto, IQueryable<TEntity>> applyDefaults
 ) : ICrudDAO<ReadDto, CreateDto, UpdateDto>
	where TEntity : BaseEntity
{
	protected AppDBContext Context => context;

	public virtual async Task<ReadDto?> FetchById(int id, CancellationToken ct)
	{
		var entity = await Context.Set<TEntity>().FindAsync([id], ct);
		if (entity is null or { IsDeleted: true }) return default;
		return mapToDto(entity);
	}

	public virtual async Task<List<ReadDto>> FetchAll(PaginationDto pagination, CancellationToken ct)
	{
		var query = Context.Set<TEntity>().AsQueryable();
		query = applyDefaults(query, pagination);
		var entities = await query.ToListAsync(ct);
		return entities.Select(mapToDto).ToList()!;
	}

	public virtual async Task<ReadDto?> Create(CreateDto dto, CancellationToken ct)
	{
		var entity = mapToEntity(dto);

		try {
			Context.Set<TEntity>().Add(entity);
			await Context.SaveChangesAsync(ct);
		} 
		catch (Exception)
        {
            return default;
        }
		
		return mapToDto(entity);
	}

	public virtual async Task<bool> Update(int id, UpdateDto dto, CancellationToken ct)
	{
		var existing = await Context.Set<TEntity>().FindAsync([id], ct);
		if (existing is null or { IsDeleted: true }) return false;

		updateEntity(existing, dto);
		return await Context.SaveChangesAsync(ct) > 0;
	}

	public virtual async Task<bool> Delete(int id, CancellationToken ct)
	{
		var entity = await Context.Set<TEntity>().FindAsync([id], ct);
		if (entity is null or { IsDeleted: true }) return false;

		entity.IsDeleted = true;
		return await Context.SaveChangesAsync(ct) > 0;
	}
}

