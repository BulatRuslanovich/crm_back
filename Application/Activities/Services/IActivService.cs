using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Services;

namespace CrmBack.Application.Activities.Services;

public interface IActivService : IService<ReadActivDto, CreateActivDto, UpdateActivDto> { }
