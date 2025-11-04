using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Dto;
using CrmBack.Infrastructure.Persistence.Activities;

namespace CrmBack.Application.Activities.Services;

/// <summary>
/// Activities service implementation
/// Handles business logic for activity operations
/// Delegates data access operations to ActivDAO
/// </summary>
public class ActivService(IActivDAO dao) : IActivService
{
    /// <summary>Get activity by ID (delegates to DAO)</summary>
    public async Task<ReadActivDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    /// <summary>Get all activities with pagination (delegates to DAO)</summary>
    public async Task<List<ReadActivDto>> GetAll(PaginationDto pagination, CancellationToken ct = default) =>
        await dao.FetchAll(pagination, ct);

    /// <summary>Create new activity (delegates to DAO)</summary>
    public async Task<ReadActivDto?> Create(CreateActivDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);

    /// <summary>Update activity (delegates to DAO)</summary>
    public async Task<bool> Update(int id, UpdateActivDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);

    /// <summary>Delete activity (soft delete via DAO)</summary>
    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);

    /// <summary>Get all activities for a specific user (delegates to DAO)</summary>
    public async Task<List<ReadActivDto>> GetByUserId(int userId, CancellationToken ct = default) =>
        await dao.FetchByUserId(userId, ct);
}
