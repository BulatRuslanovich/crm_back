using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Services;

namespace CrmBack.Application.Activities.Services;

/// <summary>
/// Activities service interface
/// Extends IService with standard CRUD operations for activities
/// </summary>
public interface IActivService : IService<ReadActivDto, CreateActivDto, UpdateActivDto> { }
