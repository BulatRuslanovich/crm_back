namespace CrmBack.Controllers;

using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
public class OrgController(IOrgService orgService)
: CrudController<ReadOrgDto, CreateOrgDto, UpdateOrgDto>(orgService)
{
    protected override int GetId(ReadOrgDto payload) => payload.OrgId;
}
