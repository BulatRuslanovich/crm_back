using CrmBack.Application.Common.Dto;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Infrastructure.Persistence.Organizations;

namespace CrmBack.Application.Organizations.Services;

/// <summary>
/// Organizations service implementation
/// Handles business logic for organization operations
/// Delegates data access operations to OrgDAO
/// </summary>
public class OrgService(IOrgDAO dao) : IOrgService
{
	/// <summary>Get organization by ID (delegates to DAO)</summary>
	public async Task<ReadOrgDto?> GetById(int id, CancellationToken ct = default) =>
		await dao.FetchById(id, ct);

	/// <summary>Get all organizations with pagination (delegates to DAO)</summary>
	public async Task<List<ReadOrgDto>> GetAll(PaginationDto pagination, CancellationToken ct = default) =>
		await dao.FetchAll(pagination, ct);

	/// <summary>Create new organization (delegates to DAO)</summary>
	public async Task<ReadOrgDto?> Create(CreateOrgDto dto, CancellationToken ct = default) =>
		await dao.Create(dto, ct);

	/// <summary>Update organization (delegates to DAO)</summary>
	public async Task<bool> Update(int id, UpdateOrgDto dto, CancellationToken ct = default) =>
		await dao.Update(id, dto, ct);

	/// <summary>Delete organization (soft delete via DAO)</summary>
	public async Task<bool> Delete(int id, CancellationToken ct = default) =>
		await dao.Delete(id, ct);
}
