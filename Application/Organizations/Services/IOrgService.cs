using CrmBack.Application.Common.Services;
using CrmBack.Application.Organizations.Dto;

namespace CrmBack.Application.Organizations.Services;

public interface IOrgService : IService<ReadOrgDto, CreateOrgDto, UpdateOrgDto> { }
