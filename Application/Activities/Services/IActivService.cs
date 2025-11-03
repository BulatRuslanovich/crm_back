using CrmBack.Application.Common.Services;
using CrmBack.Application.Activities.Dto;

namespace CrmBack.Application.Activities.Services;

public interface IActivService : IService<ReadActivDto, CreateActivDto, UpdateActivDto> { }
