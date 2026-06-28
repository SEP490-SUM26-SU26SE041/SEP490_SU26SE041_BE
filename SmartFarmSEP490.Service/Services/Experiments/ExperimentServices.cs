using M = SmartFarmSEP490.Model;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Beds;
using SmartFarmSEP490.Repository.Interfaces.CareSchedules;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Interfaces.ExperimentDesigns;
using SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;
using SmartFarmSEP490.Repository.Interfaces.ProcedureTemplates;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.Service.Services.Experiments;

public class ExperimentService : IExperimentService
{
    private readonly IExperimentRepository _experimentRepository;
    private readonly IProcedureTemplateRepository _templateRepository;
    private readonly IExperimentRequestRepository _requestRepository;
    private readonly IExperimentBedAssignmentRepository _bedAssignmentRepository;
    private readonly IBedRepository _bedRepository;

    public ExperimentService(
        IExperimentRepository experimentRepository,
        IProcedureTemplateRepository templateRepository,
        IExperimentRequestRepository requestRepository,
        IExperimentBedAssignmentRepository bedAssignmentRepository,
        IBedRepository bedRepository)
    {
        _experimentRepository = experimentRepository;
        _templateRepository = templateRepository;
        _requestRepository = requestRepository;
        _bedAssignmentRepository = bedAssignmentRepository;
        _bedRepository = bedRepository;
    }

    public async Task<ExperimentResponseDto?> CreateAsync(CreateExperimentDto dto, Guid researcherId)
    {
        try
        {
            if (dto.RequestId.HasValue)
            {
                var request = await _requestRepository.GetByIdAsync(dto.RequestId.Value);
                if (request == null)
                    throw new InvalidOperationException($"Khong tim thay yeu cau thuc nghiem voi ID: {dto.RequestId}");
                if (request.Status != RequestStatus.Approved)
                    throw new InvalidOperationException($"Yeu cau thuc nghiem phai co trang thai 'Approved' de co the tao thuc nghiem. Trang thai hien tai: '{request.Status}'");
            }

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
                Status = ExperimentStatus.Draft
            };

            if (dto.ProcedureTemplateId.HasValue)
            {
                var template = await _templateRepository.GetByIdWithStepsAsync(dto.ProcedureTemplateId.Value);
                if (template != null && template.ProcedureTemplateSteps != null && template.ProcedureTemplateSteps.Count > 0)
                {
                    var stages = template.ProcedureTemplateSteps
                        .OrderBy(s => s.StepOrder)
                        .Select(s => new M.ExperimentStage
                        {
                            StageOrder = s.StepOrder,
                            StageName = s.Title,
                            StageType = s.StageType,
                            Objective = s.Instruction
                        })
                        .ToList();
                    var result = await _experimentRepository.CreateWithStagesAsync(entity, stages);
                    if (dto.RequestId.HasValue)
                        await _bedAssignmentRepository.AssignBedsToExperimentAsync(dto.RequestId.Value, result.Id);
                    return await GetByIdAsync(result.Id);
                }
            }

            var resultOnly = await _experimentRepository.CreateAsync(entity);
            if (dto.RequestId.HasValue)
                await _bedAssignmentRepository.AssignBedsToExperimentAsync(dto.RequestId.Value, resultOnly.Id);
            return await GetByIdAsync(resultOnly.Id);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentResponseDto?> CreateFromRequestAsync(Guid requestId, Guid researcherId)
    {
        try
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new InvalidOperationException($"Khong tim thay yeu cau thuc nghiem voi ID: {requestId}");
            if (request.Status != RequestStatus.Approved)
                throw new InvalidOperationException($"Yeu cau thuc nghiem phai co trang thai 'Approved' de co the tao thuc nghiem. Trang thai hien tai: '{request.Status}'");

            var entity = new M.Experiment
            {
                RequestId = requestId,
                FarmId = request.FarmId,
                ResearcherId = researcherId,
                CropVarietyId = request.CropVarietyId,
                ProcedureTemplateId = request.ProcedureTemplateId,
                Title = request.Title,
                Objective = request.Objective,
                Status = ExperimentStatus.Draft
            };

            if (request.ProcedureTemplateId.HasValue)
            {
                var template = await _templateRepository.GetByIdWithStepsAsync(request.ProcedureTemplateId.Value);
                if (template != null && template.ProcedureTemplateSteps?.Count > 0)
                {
                    var stages = template.ProcedureTemplateSteps
                        .OrderBy(s => s.StepOrder)
                        .Select(s => new M.ExperimentStage
                        {
                            StageOrder = s.StepOrder, StageName = s.Title,
                            StageType = s.StageType, Objective = s.Instruction
                        }).ToList();
                    var created = await _experimentRepository.CreateWithStagesAsync(entity, stages);
                    await _bedAssignmentRepository.AssignBedsToExperimentAsync(requestId, created.Id);
                    return await GetByIdAsync(created.Id);
                }
            }

            var resultOnly = await _experimentRepository.CreateAsync(entity);
            await _bedAssignmentRepository.AssignBedsToExperimentAsync(requestId, resultOnly.Id);
            return await GetByIdAsync(resultOnly.Id);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao thuc nghiem tu yeu cau that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
            if (dto.Status != null) entity.Status = Enum.Parse<ExperimentStatus>(dto.Status);
            await _experimentRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentResponseDto?> UpdateStatusAsync(Guid id, string status, Guid researcherId)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdAsync(id);
            if (entity == null) return null;
            entity.Status = Enum.Parse<ExperimentStatus>(status);
            await _experimentRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat trang thai thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;
            return MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay thong tin thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _experimentRepository.GetAllAsync();
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentResponseDto>> GetByResearcherAsync(Guid researcherId)
    {
        try
        {
            var entities = await _experimentRepository.GetByResearcherAsync(researcherId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach thuc nghiem theo nha nghien cuu that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentResponseDto>> GetByFarmAsync(Guid farmId)
    {
        try
        {
            var entities = await _experimentRepository.GetByFarmAsync(farmId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach thuc nghiem theo trai that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _experimentRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _bedAssignmentRepository.ReleaseBedsAsync(id);
            await _experimentRepository.DeleteAsync(id);
            return true;
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
            Status = entity.Status.ToString(),
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
                CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt, StageType = s.StageType.ToString()
            }).ToList() ?? new(),
            Groups = entity.ExperimentGroups?.Select(g => new ExperimentGroupResponseDto
            {
                Id = g.Id, GroupName = g.GroupName, TreatmentDescription = g.TreatmentDescription, GroupType = g.GroupType.ToString(), CreatedAt = g.CreatedAt
            }).ToList() ?? new(),
            MeasurementDefinitions = entity.MeasurementDefinitions?.Select(m => new MeasurementDefinitionResponseDto
            {
                Id = m.Id, GroupId = m.GroupId, GroupName = m.Group?.GroupName,
                MetricName = m.MetricName, Unit = m.Unit, TargetValue = m.TargetValue, Description = m.Description
            }).ToList() ?? new(),
            Design = entity.ExperimentDesign != null ? new ExperimentDesignResponseDto
            {
                Id = entity.ExperimentDesign.Id,
                DesignType = entity.ExperimentDesign.DesignType.ToString(),
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
                EndDate = dto.EndDate,
                StageType = dto.StageType
            };
            var result = await _stageRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao giai doan thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
            if (dto.StageType.HasValue) entity.StageType = dto.StageType.Value;
            await _stageRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat giai doan thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentStageResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _stageRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach giai doan that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa giai doan that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    private static ExperimentStageResponseDto MapToResponseDto(M.ExperimentStage entity)
    {
        return new ExperimentStageResponseDto
        {
            Id = entity.Id, StageName = entity.StageName, StageOrder = entity.StageOrder,
            Objective = entity.Objective, StartDate = entity.StartDate, EndDate = entity.EndDate,
            ResultSummary = entity.ResultSummary, ResultData = entity.ResultData,
            CreatedAt = entity.CreatedAt, UpdatedAt = entity.UpdatedAt, StageType = entity.StageType.ToString()
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
                TreatmentDescription = dto.TreatmentDescription,
                GroupType = dto.GroupType
            };
            var result = await _groupRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao nhom thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentGroupResponseDto?> UpdateAsync(Guid id, UpdateExperimentGroupDto dto)
    {
        try
        {
            var entity = await _groupRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.GroupName != null) entity.GroupName = dto.GroupName;
            if (dto.TreatmentDescription != null) entity.TreatmentDescription = dto.TreatmentDescription;
            if (dto.GroupType.HasValue) entity.GroupType = dto.GroupType.Value;
            await _groupRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat nhom thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentGroupResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _groupRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach nhom that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
        catch (Exception ex) { throw new Exception($"Xoa nhom that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    private static ExperimentGroupResponseDto MapToResponseDto(M.ExperimentGroup entity)
    {
        return new ExperimentGroupResponseDto
        {
            Id = entity.Id, GroupName = entity.GroupName, TreatmentDescription = entity.TreatmentDescription, GroupType = entity.GroupType.ToString(), CreatedAt = entity.CreatedAt
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
            if (dto.ReplicationCount.HasValue && dto.ReplicationCount < 2)
                throw new InvalidOperationException("ReplicationCount phai lon hon hoac bang 2 de dam bao y nghia thong ke.");

            var entity = new M.ExperimentDesign
            {
                ExperimentId = experimentId,
                DesignType = dto.DesignType,
                ReplicationCount = dto.ReplicationCount,
                RandomizationMethod = dto.RandomizationMethod,
                DesignParameters = dto.DesignParameters
            };
            var result = await _designRepository.CreateAsync(entity);
            return MapToResponseDto(result);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao thiet ke that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentDesignResponseDto?> UpdateAsync(Guid id, UpdateExperimentDesignDto dto)
    {
        try
        {
            var entity = await _designRepository.GetByExperimentAsync(id);
            if (entity == null) return null;
            if (dto.DesignType.HasValue) entity.DesignType = dto.DesignType.Value;
            if (dto.ReplicationCount.HasValue)
            {
                if (dto.ReplicationCount < 2)
                    throw new InvalidOperationException("ReplicationCount phai lon hon hoac bang 2 de dam bao y nghia thong ke.");
                entity.ReplicationCount = dto.ReplicationCount;
            }
            if (dto.RandomizationMethod != null) entity.RandomizationMethod = dto.RandomizationMethod;
            if (dto.DesignParameters != null) entity.DesignParameters = dto.DesignParameters;
            await _designRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat thiet ke that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentDesignResponseDto?> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entity = await _designRepository.GetByExperimentAsync(experimentId);
            return entity != null ? MapToResponseDto(entity) : null;
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay thiet ke that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<bool> DeleteAsync(Guid experimentId)
    {
        try
        {
            await _designRepository.DeleteAsync(experimentId);
            return true;
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa thiet ke that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    private static ExperimentDesignResponseDto MapToResponseDto(M.ExperimentDesign entity)
    {
        return new ExperimentDesignResponseDto
        {
            Id = entity.Id, DesignType = entity.DesignType.ToString(), ReplicationCount = entity.ReplicationCount,
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao chi so do luong that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat chi so do luong that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<MeasurementDefinitionResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _measurementRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach chi so do luong that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa chi so do luong that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
                Id = Guid.NewGuid(),
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
                    RequiredSkillDescription = s.RequiredSkillDescription,
                    StageType = s.StageType ?? ExperimentStageType.Other
                }).ToList();
            }
            var result = await _templateRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException ex) { throw new Exception($"Tao mau quy trinh that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
        catch (Exception ex) { throw new Exception($"Tao mau quy trinh that bai: {ex.InnerException?.Message ?? ex.Message}"); }
    }

    public async Task<ProcedureTemplateResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _templateRepository.GetByIdWithStepsAsync(id);
            if (entity == null) return null;
            return MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay thong tin mau quy trinh that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ProcedureTemplateResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _templateRepository.GetAllAsync();
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach mau quy trinh that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ProcedureTemplateResponseDto>> GetByCropVarietyAsync(Guid cropVarietyId)
    {
        try
        {
            var entities = await _templateRepository.GetByCropVarietyAsync(cropVarietyId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay mau quy trinh theo giong that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa mau quy trinh that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
                ExpectedDurationDays = s.ExpectedDurationDays, RequiredSkillDescription = s.RequiredSkillDescription,
                StageType = s.StageType.ToString()
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
                TaskType = dto.TaskType,
                Instruction = dto.Instruction,
                FrequencyDays = dto.FrequencyDays,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };
            var result = await _careScheduleRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao lich cham soc that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
            if (dto.TaskType != null) entity.TaskType = dto.TaskType;
            if (dto.Instruction != null) entity.Instruction = dto.Instruction;
            if (dto.FrequencyDays.HasValue) entity.FrequencyDays = dto.FrequencyDays;
            if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate;
            await _careScheduleRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat lich cham soc that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<CareScheduleResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _careScheduleRepository.GetByExperimentAsync(experimentId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach lich cham soc that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa lich cham soc that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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