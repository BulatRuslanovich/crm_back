using CrmBack.Core.Models.Dto;


namespace CrmBack.DAO;

public interface IPlanDAO : ICrudDAO<ReadPlanDto, CreatePlanDto, UpdatePlanDto> { }
