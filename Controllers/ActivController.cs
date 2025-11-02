
using CrmBack.Core.Models.Dto;
using CrmBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmBack.Controllers;

[ApiVersion("1.0")]
public class ActivController(IActivService activService)
: CrudController<ReadActivDto, CreateActivDto, UpdateActivDto>(activService)
{ }
