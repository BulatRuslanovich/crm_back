using CrmBack.Application.Common.Services;
using CrmBack.Application.Organizations.Dto;

namespace CrmBack.Application.Organizations.Services;

/// <summary>
/// Organizations service interface
/// Extends IService with standard CRUD operations for organizations
/// </summary>
public interface IOrgService : IService<ReadOrgDto, CreateOrgDto, UpdateOrgDto> { }
