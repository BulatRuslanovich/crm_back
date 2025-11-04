namespace CrmBack.Api.Controllers.Activities;

using CrmBack.Api.Controllers.Base;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Activities.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Activities (Activ) management controller
/// Provides CRUD operations for activities via CrudController base class
/// Security: Inherits authorization requirements from CrudController (Authorize attribute)
/// </summary>
[ApiVersion("1.0")]
public class ActivController(IActivService activService, ILogger<ActivController> logger) : CrudController<ReadActivDto, CreateActivDto, UpdateActivDto>(activService, logger)
{
    /// <summary>
    /// Implements abstract method from CrudController
    /// Extracts activity ID from read DTO
    /// </summary>
    protected override int GetId(ReadActivDto payload) => payload.ActivId;
}
