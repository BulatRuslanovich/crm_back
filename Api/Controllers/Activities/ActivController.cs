namespace CrmBack.Api.Controllers.Activities;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Activities.Services;
public class ActivController(IActivService activService) : CrudController<ReadActivDto, CreateActivDto, UpdateActivDto>(activService)
{
	protected override int GetId(ReadActivDto payload) => payload.ActivId;
}
