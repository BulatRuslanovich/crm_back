using CrmBack.Application.Organizations.Dto;
using CrmBack.Infrastructure.Persistence.Common;

namespace CrmBack.Infrastructure.Persistence.Organizations;

public interface IOrgDAO : ICrudDAO<ReadOrgDto, CreateOrgDto, UpdateOrgDto> { }
