using CrmBack.Application.Organizations.Dto;
using CrmBack.Infrastructure.Persistence.Common;

namespace CrmBack.Infrastructure.Persistence.Organizations;

/// <summary>
/// Organizations Data Access Object interface
/// Extends ICrudDAO with standard CRUD operations for organizations
/// </summary>
public interface IOrgDAO : ICrudDAO<ReadOrgDto, CreateOrgDto, UpdateOrgDto> { }
