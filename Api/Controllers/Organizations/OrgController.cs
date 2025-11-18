namespace CrmBack.Api.Controllers.Organizations;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Organizations.Dto;
using CrmBack.Application.Organizations.Services;

public class OrgController(IOrgService orgService) : CrudController<ReadOrgDto, CreateOrgDto, UpdateOrgDto>(orgService)
{
	protected override int GetId(ReadOrgDto payload) => payload.OrgId;
}
