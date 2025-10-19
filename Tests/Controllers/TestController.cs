using CrmBack.Controllers;
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Services;

namespace Tests.Controllers;

public class TestController(IOrgService orgService) : BaseApiController<ReadOrgPayload, CreateOrgPayload, UpdateOrgPayload>(orgService)
{
    public new bool ValidateId(int id) => base.ValidateId(id);
}
