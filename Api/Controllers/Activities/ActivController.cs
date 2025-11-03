namespace CrmBack.Api.Controllers.Activities;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Activities.Services;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
public class ActivController(IActivService activService, ILogger<ActivController> logger) : CrudController<ReadActivDto, CreateActivDto, UpdateActivDto>(activService, logger)
{
    protected override int GetId(ReadActivDto payload) => payload.ActivId;
}
