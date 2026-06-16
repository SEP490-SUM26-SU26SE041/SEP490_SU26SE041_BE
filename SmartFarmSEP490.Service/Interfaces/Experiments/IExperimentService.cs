using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Experiments;

public interface IExperimentService
{
    Task<ExperimentResponseDto?> CreateAsync(CreateExperimentDto dto, Guid researcherId);
    Task<ExperimentResponseDto?> UpdateAsync(Guid id, UpdateExperimentDto dto, Guid researcherId);
    Task<ExperimentResponseDto?> UpdateStatusAsync(Guid id, string status, Guid researcherId);
    Task<ExperimentResponseDto?> GetByIdAsync(Guid id);
    Task<List<ExperimentResponseDto>> GetAllAsync();
    Task<List<ExperimentResponseDto>> GetByResearcherAsync(Guid researcherId);
    Task<List<ExperimentResponseDto>> GetByFarmAsync(Guid farmId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IExperimentStageService
{
    Task<ExperimentStageResponseDto?> CreateAsync(Guid experimentId, CreateExperimentStageDto dto);
    Task<ExperimentStageResponseDto?> UpdateAsync(Guid id, UpdateExperimentStageDto dto);
    Task<List<ExperimentStageResponseDto>> GetByExperimentAsync(Guid experimentId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IExperimentGroupService
{
    Task<ExperimentGroupResponseDto?> CreateAsync(Guid experimentId, CreateExperimentGroupDto dto);
    Task<ExperimentGroupResponseDto?> UpdateAsync(Guid id, UpdateExperimentGroupDto dto);
    Task<List<ExperimentGroupResponseDto>> GetByExperimentAsync(Guid experimentId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IExperimentDesignService
{
    Task<ExperimentDesignResponseDto?> CreateAsync(Guid experimentId, CreateExperimentDesignDto dto);
    Task<ExperimentDesignResponseDto?> UpdateAsync(Guid id, UpdateExperimentDesignDto dto);
    Task<ExperimentDesignResponseDto?> GetByExperimentAsync(Guid experimentId);
    Task<bool> DeleteAsync(Guid experimentId);
}

public interface IMeasurementDefinitionService
{
    Task<MeasurementDefinitionResponseDto?> CreateAsync(Guid experimentId, CreateMeasurementDefinitionDto dto);
    Task<MeasurementDefinitionResponseDto?> UpdateAsync(Guid id, UpdateMeasurementDefinitionDto dto);
    Task<List<MeasurementDefinitionResponseDto>> GetByExperimentAsync(Guid experimentId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IProcedureTemplateService
{
    Task<ProcedureTemplateResponseDto?> CreateAsync(CreateProcedureTemplateDto dto, Guid createdById);
    Task<ProcedureTemplateResponseDto?> GetByIdAsync(Guid id);
    Task<List<ProcedureTemplateResponseDto>> GetAllAsync();
    Task<List<ProcedureTemplateResponseDto>> GetByCropVarietyAsync(Guid cropVarietyId);
    Task<bool> DeleteAsync(Guid id);
}

public interface ICareScheduleService
{
    Task<CareScheduleResponseDto?> CreateAsync(Guid experimentId, CreateCareScheduleDto dto);
    Task<CareScheduleResponseDto?> UpdateAsync(Guid id, UpdateCareScheduleDto dto);
    Task<List<CareScheduleResponseDto>> GetByExperimentAsync(Guid experimentId);
    Task<bool> DeleteAsync(Guid id);
}
