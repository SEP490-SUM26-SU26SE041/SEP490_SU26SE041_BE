using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.CareSchedules;
using SmartFarmSEP490.Repository.Interfaces.ExperimentDesigns;
using SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;
using SmartFarmSEP490.Repository.Interfaces.ProcedureTemplates;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.Service.Services.Experiments;

public class ExperimentService : IExperimentService
{
    private readonly IExperimentRepository _experimentRepository;

    public ExperimentService(IExperimentRepository experimentRepository)
    {
        _experimentRepository = experimentRepository;
    }

    public async Task<ExperimentResponseDto?> CreateAsync(CreateExperimentDto dto, Guid researcherId)
    {
        try
        {
            var entity = new M.Experiment
            {
                RequestId = dto.RequestId,
                FarmId = dto.FarmId,
                ResearcherId = researcherId,
                CropVarietyId = dto.CropVarietyId,
                ProcedureTemplateId = dto.ProcedureTemplateId,
                ExperimentCode = dto.ExperimentCode,
                Title = dto.Title,
                Objective = dto.Objective,
                Hypothesis = dto.Hypothesis,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Draft"
            };
            var result = await _experimentRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (Exception ex) { throw new Exception($"Create experiment failed: {ex.Message}"); }
    }

    public async Task<ExperimentResponseDto?> UpdateAsync(Guid id, UpdateExperimentDto dto, Guid researcherId)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.ExperimentCode != null) entity.ExperimentCode = dto.ExperimentCode;
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Objective != null) entity.Objective = dto.Objective;
            if (dto.Hypothesis != null) entity.Hypothesis = dto.Hypothesis;
            if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate;
            if (dto.Status != null) entity.Status = dto.Status;
            await _experimentRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update experiment failed: {ex.Message}"); }
    }

    public async Task<ExperimentResponseDto?> UpdateStatusAsync(Guid id, string status, Guid researcherId)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdAsync(id);
            if (entity == null) return null;
            entity.Status = status;
            await _experimentRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update experiment status failed: {ex.Message}"); }
    }

    public async Task<ExperimentResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get experiment failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _experimentRepository.GetAllAsync();
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get all experiments failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentResponseDto>> GetByResearcherAsync(Guid researcherId)
    {
        try
        {
            var entities = await _experimentRepository.GetByResearcherAsync(researcherId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiments by researcher failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentResponseDto>> GetByFarmAsync(Guid farmId)
    {
        try
        {
            var entities = await _experimentRepository.GetByFarmAsync(farmId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiments by farm failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _experimentRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete experiment failed: {ex.Message}"); }
    }

    private static ExperimentResponseDto MapToResponseDto(M.Experiment entity)
    {
        return new ExperimentResponseDto
        {
            Id = entity.Id,
            ExperimentCode = entity.ExperimentCode,
            Title = entity.Title,
            Objective = entity.Objective,
            Hypothesis = entity.Hypothesis,
            Status = entity.Status,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            RequestId = entity.RequestId,
            FarmId = entity.FarmId,
            FarmName = entity.Farm?.FarmName,
            ResearcherId = entity.ResearcherId,
            ResearcherName = entity.Researcher?.FullName,
            CropVarietyId = entity.CropVarietyId,
            CropVarietyName = entity.CropVariety?.VarietyName,
            ProcedureTemplateId = entity.ProcedureTemplateId,
            ProcedureTemplateName = entity.ProcedureTemplate?.TemplateName,
            Stages = entity.ExperimentStages?.Select(s => new ExperimentStageResponseDto
            {
                Id = s.Id, StageName = s.StageName, StageOrder = s.StageOrder,
                Objective = s.Objective, StartDate = s.StartDate, EndDate = s.EndDate,
                ResultSummary = s.ResultSummary, ResultData = s.ResultData,
                CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt
            }).ToList() ?? new(),
            Groups = entity.ExperimentGroups?.Select(g => new ExperimentGroupResponseDto
            {
                Id = g.Id, GroupName = g.GroupName, TreatmentDescription = g.TreatmentDescription, CreatedAt = g.CreatedAt
            }).ToList() ?? new(),
            MeasurementDefinitions = entity.MeasurementDefinitions?.Select(m => new MeasurementDefinitionResponseDto
            {
                Id = m.Id, GroupId = m.GroupId, GroupName = m.Group?.GroupName,
                MetricName = m.MetricName, Unit = m.Unit, TargetValue = m.TargetValue, Description = m.Description
            }).ToList() ?? new(),
            Design = entity.ExperimentDesign != null ? new ExperimentDesignResponseDto
            {
                Id = entity.ExperimentDesign.Id,
                ReplicationCount = entity.ExperimentDesign.ReplicationCount,
                RandomizationMethod = entity.ExperimentDesign.RandomizationMethod,
                DesignParameters = entity.ExperimentDesign.DesignParameters
            } : null
        };
    }
}

public class ExperimentStageService : IExperimentStageService
{
    private readonly IExperimentStageRepository _stageRepository;

    public ExperimentStageService(IExperimentStageRepository stageRepository)
    {
        _stageRepository = stageRepository;
    }

    public async Task<ExperimentStageResponseDto?> CreateAsync(Guid experimentId, CreateExperimentStageDto dto)
    {
        try
        {
            var entity = new M.ExperimentStage
            {
                ExperimentId = experimentId,
                StageName = dto.StageName,
                StageOrder = dto.StageOrder,
                Objective = dto.Objective,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };
            var result = await _stageRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (Exception ex) { throw new Exception($"Create experiment stage failed: {ex.Message}"); }
    }

    public async Task<ExperimentStageResponseDto?> UpdateAsync(Guid id, UpdateExperimentStageDto dto)
    {
        try
        {
            var entity = await _stageRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.StageName != null) entity.StageName = dto.StageName;
            if (dto.StageOrder.HasValue) entity.StageOrder = dto.StageOrder.Value;
            if (dto.Objective != null) entity.Objective = dto.Objective;
            if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate;
            if (dto.ResultSummary != null) entity.ResultSummary = dto.ResultSummary;
            if (dto.ResultData != null) entity.ResultData = dto.ResultData;
            await _stageRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Update experiment stage failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentStageResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _stageRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiment stages failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _stageRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _stageRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete experiment stage failed: {ex.Message}"); }
    }

    private static ExperimentStageResponseDto MapToResponseDto(M.ExperimentStage entity)
    {
        return new ExperimentStageResponseDto
        {
            Id = entity.Id, StageName = entity.StageName, StageOrder = entity.StageOrder,
            Objective = entity.Objective, StartDate = entity.StartDate, EndDate = entity.EndDate,
            ResultSummary = entity.ResultSummary, ResultData = entity.ResultData,
            CreatedAt = entity.CreatedAt, UpdatedAt = entity.UpdatedAt
        };
    }
}

public class ExperimentGroupService : IExperimentGroupService
{
    private readonly IExperimentGroupRepository _groupRepository;

    public ExperimentGroupService(IExperimentGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<ExperimentGroupResponseDto?> CreateAsync(Guid experimentId, CreateExperimentGroupDto dto)
    {
        try
        {
            var entity = new M.ExperimentGroup
            {
                ExperimentId = experimentId,
                GroupName = dto.GroupName,
                TreatmentDescription = dto.TreatmentDescription
            };
            var result = await _groupRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (Exception ex) { throw new Exception($"Create experiment group failed: {ex.Message}"); }
    }

    public async Task<ExperimentGroupResponseDto?> UpdateAsync(Guid id, UpdateExperimentGroupDto dto)
    {
        try
        {
            var entity = await _groupRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.GroupName != null) entity.GroupName = dto.GroupName;
            if (dto.TreatmentDescription != null) entity.TreatmentDescription = dto.TreatmentDescription;
            await _groupRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Update experiment group failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentGroupResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _groupRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiment groups failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _groupRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _groupRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete experiment group failed: {ex.Message}"); }
    }

    private static ExperimentGroupResponseDto MapToResponseDto(M.ExperimentGroup entity)
    {
        return new ExperimentGroupResponseDto
        {
            Id = entity.Id, GroupName = entity.GroupName, TreatmentDescription = entity.TreatmentDescription, CreatedAt = entity.CreatedAt
        };
    }
}

public class ExperimentDesignService : IExperimentDesignService
{
    private readonly IExperimentDesignRepository _designRepository;

    public ExperimentDesignService(IExperimentDesignRepository designRepository)
    {
        _designRepository = designRepository;
    }

    public async Task<ExperimentDesignResponseDto?> CreateAsync(Guid experimentId, CreateExperimentDesignDto dto)
    {
        try
        {
            var entity = new M.ExperimentDesign
            {
                ExperimentId = experimentId,
                ReplicationCount = dto.ReplicationCount,
                RandomizationMethod = dto.RandomizationMethod,
                DesignParameters = dto.DesignParameters
            };
            var result = await _designRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (Exception ex) { throw new Exception($"Create experiment design failed: {ex.Message}"); }
    }

    public async Task<ExperimentDesignResponseDto?> UpdateAsync(Guid id, UpdateExperimentDesignDto dto)
    {
        try
        {
            var entity = await _designRepository.GetByExperimentAsync(id);
            if (entity == null) return null;
            if (dto.ReplicationCount.HasValue) entity.ReplicationCount = dto.ReplicationCount;
            if (dto.RandomizationMethod != null) entity.RandomizationMethod = dto.RandomizationMethod;
            if (dto.DesignParameters != null) entity.DesignParameters = dto.DesignParameters;
            await _designRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Update experiment design failed: {ex.Message}"); }
    }

    public async Task<ExperimentDesignResponseDto?> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entity = await _designRepository.GetByExperimentAsync(experimentId);
            return entity != null ? MapToResponseDto(entity) : null;
        }
        catch (Exception ex) { throw new Exception($"Get experiment design failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid experimentId)
    {
        try
        {
            await _designRepository.DeleteAsync(experimentId);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete experiment design failed: {ex.Message}"); }
    }

    private static ExperimentDesignResponseDto MapToResponseDto(M.ExperimentDesign entity)
    {
        return new ExperimentDesignResponseDto
        {
            Id = entity.Id, ReplicationCount = entity.ReplicationCount,
            RandomizationMethod = entity.RandomizationMethod, DesignParameters = entity.DesignParameters
        };
    }
}

public class MeasurementDefinitionService : IMeasurementDefinitionService
{
    private readonly IMeasurementDefinitionRepository _measurementRepository;

    public MeasurementDefinitionService(IMeasurementDefinitionRepository measurementRepository)
    {
        _measurementRepository = measurementRepository;
    }

    public async Task<MeasurementDefinitionResponseDto?> CreateAsync(Guid experimentId, CreateMeasurementDefinitionDto dto)
    {
        try
        {
            var entity = new M.MeasurementDefinition
            {
                ExperimentId = experimentId,
                GroupId = dto.GroupId,
                MetricName = dto.MetricName,
                Unit = dto.Unit,
                TargetValue = dto.TargetValue,
                Description = dto.Description
            };
            var result = await _measurementRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (Exception ex) { throw new Exception($"Create measurement definition failed: {ex.Message}"); }
    }

    public async Task<MeasurementDefinitionResponseDto?> UpdateAsync(Guid id, UpdateMeasurementDefinitionDto dto)
    {
        try
        {
            var entity = await _measurementRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.GroupId.HasValue) entity.GroupId = dto.GroupId;
            if (dto.MetricName != null) entity.MetricName = dto.MetricName;
            if (dto.Unit != null) entity.Unit = dto.Unit;
            if (dto.TargetValue.HasValue) entity.TargetValue = dto.TargetValue;
            if (dto.Description != null) entity.Description = dto.Description;
            await _measurementRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Update measurement definition failed: {ex.Message}"); }
    }

    public async Task<List<MeasurementDefinitionResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _measurementRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get measurement definitions failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _measurementRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _measurementRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete measurement definition failed: {ex.Message}"); }
    }

    private static MeasurementDefinitionResponseDto MapToResponseDto(M.MeasurementDefinition entity)
    {
        return new MeasurementDefinitionResponseDto
        {
            Id = entity.Id, GroupId = entity.GroupId, GroupName = entity.Group?.GroupName,
            MetricName = entity.MetricName, Unit = entity.Unit, TargetValue = entity.TargetValue, Description = entity.Description
        };
    }
}

public class ProcedureTemplateService : IProcedureTemplateService
{
    private readonly IProcedureTemplateRepository _templateRepository;

    public ProcedureTemplateService(IProcedureTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<ProcedureTemplateResponseDto?> CreateAsync(CreateProcedureTemplateDto dto, Guid createdById)
    {
        try
        {
            var entity = new M.ProcedureTemplate
            {
                CropVarietyId = dto.CropVarietyId,
                TemplateName = dto.TemplateName,
                Objective = dto.Objective,
                Description = dto.Description,
                CreatedBy = createdById
            };
            if (dto.Steps != null && dto.Steps.Count > 0)
            {
                entity.ProcedureTemplateSteps = dto.Steps.Select(s => new M.ProcedureTemplateStep
                {
                    StepOrder = s.StepOrder,
                    Title = s.Title,
                    Instruction = s.Instruction,
                    ExpectedDurationDays = s.ExpectedDurationDays,
                    RequiredSkillDescription = s.RequiredSkillDescription
                }).ToList();
            }
            var result = await _templateRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (Exception ex) { throw new Exception($"Create procedure template failed: {ex.Message}"); }
    }

    public async Task<ProcedureTemplateResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _templateRepository.GetByIdWithStepsAsync(id);
            if (entity == null) return null;
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get procedure template failed: {ex.Message}"); }
    }

    public async Task<List<ProcedureTemplateResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _templateRepository.GetAllAsync();
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get all procedure templates failed: {ex.Message}"); }
    }

    public async Task<List<ProcedureTemplateResponseDto>> GetByCropVarietyAsync(Guid cropVarietyId)
    {
        try
        {
            var entities = await _templateRepository.GetByCropVarietyAsync(cropVarietyId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get procedure templates by crop variety failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _templateRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _templateRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete procedure template failed: {ex.Message}"); }
    }

    private static ProcedureTemplateResponseDto MapToResponseDto(M.ProcedureTemplate entity)
    {
        return new ProcedureTemplateResponseDto
        {
            Id = entity.Id, TemplateName = entity.TemplateName, Objective = entity.Objective,
            Description = entity.Description, CropVarietyId = entity.CropVarietyId,
            CropVarietyName = entity.CropVariety?.VarietyName, CreatedAt = entity.CreatedAt,
            Steps = entity.ProcedureTemplateSteps?.Select(s => new ProcedureTemplateStepResponseDto
            {
                Id = s.Id, StepOrder = s.StepOrder, Title = s.Title, Instruction = s.Instruction,
                ExpectedDurationDays = s.ExpectedDurationDays, RequiredSkillDescription = s.RequiredSkillDescription
            }).ToList() ?? new()
        };
    }
}

public class CareScheduleService : ICareScheduleService
{
    private readonly ICareScheduleRepository _careScheduleRepository;
    private readonly IExperimentRepository _experimentRepository;

    public CareScheduleService(ICareScheduleRepository careScheduleRepository, IExperimentRepository experimentRepository)
    {
        _careScheduleRepository = careScheduleRepository;
        _experimentRepository = experimentRepository;
    }

    public async Task<CareScheduleResponseDto?> CreateAsync(Guid experimentId, CreateCareScheduleDto dto)
    {
        try
        {
            var entity = new M.CareSchedule
            {
                ExperimentId = experimentId,
                ExperimentStageId = dto.ExperimentStageId,
                BatchId = dto.BatchId,
                Title = dto.Title,
                Instruction = dto.Instruction,
                FrequencyDays = dto.FrequencyDays,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };
            var result = await _careScheduleRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (Exception ex) { throw new Exception($"Create care schedule failed: {ex.Message}"); }
    }

    public async Task<CareScheduleResponseDto?> UpdateAsync(Guid id, UpdateCareScheduleDto dto)
    {
        try
        {
            var entity = await _careScheduleRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.ExperimentStageId.HasValue) entity.ExperimentStageId = dto.ExperimentStageId;
            if (dto.BatchId.HasValue) entity.BatchId = dto.BatchId;
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Instruction != null) entity.Instruction = dto.Instruction;
            if (dto.FrequencyDays.HasValue) entity.FrequencyDays = dto.FrequencyDays;
            if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate;
            await _careScheduleRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update care schedule failed: {ex.Message}"); }
    }

    public async Task<List<CareScheduleResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _careScheduleRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get care schedules failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _careScheduleRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _careScheduleRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete care schedule failed: {ex.Message}"); }
    }

    private async Task<CareScheduleResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _careScheduleRepository.GetByIdAsync(id);
        return entity != null ? MapToResponseDto(entity) : null;
    }

    private static CareScheduleResponseDto MapToResponseDto(M.CareSchedule entity)
    {
        return new CareScheduleResponseDto
        {
            Id = entity.Id, Title = entity.Title, Instruction = entity.Instruction,
            FrequencyDays = entity.FrequencyDays, StartDate = entity.StartDate, EndDate = entity.EndDate,
            CreatedAt = entity.CreatedAt, ExperimentId = entity.ExperimentStage?.ExperimentId ?? Guid.Empty,
            ExperimentStageId = entity.ExperimentStageId, ExperimentStageName = entity.ExperimentStage?.StageName,
            BatchId = entity.BatchId, BatchCode = entity.Batch?.BatchCode
        };
    }
}