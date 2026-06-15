using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Service.Interfaces.ExperimentRequests;

namespace SmartFarmSEP490.Service.Services.ExperimentRequests;

public class ExperimentRequestService : IExperimentRequestService
{
    private readonly IExperimentRequestRepository _requestRepository;
    private readonly IRequestReviewRepository _reviewRepository;

    public ExperimentRequestService(IExperimentRequestRepository requestRepository, IRequestReviewRepository reviewRepository)
    {
        _requestRepository = requestRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<ExperimentRequestResponseDto?> CreateAsync(CreateExperimentRequestDto dto, Guid researcherId)
    {
        try
        {
            var entity = new M.ExperimentRequest
            {
                FarmId = dto.FarmId,
                ResearcherId = researcherId,
                CropVarietyId = dto.CropVarietyId,
                ProcedureTemplateId = dto.ProcedureTemplateId,
                Title = dto.Title,
                Objective = dto.Objective,
                ExpectedStartDate = dto.ExpectedStartDate,
                ExpectedEndDate = dto.ExpectedEndDate,
                MonitoringPlan = dto.MonitoringPlan,
                Status = "Pending"
            };
            var result = await _requestRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (Exception ex) { throw new Exception($"Create experiment request failed: {ex.Message}"); }
    }

    public async Task<ExperimentRequestResponseDto?> UpdateAsync(Guid id, UpdateExperimentRequestDto dto, Guid researcherId)
    {
        try
        {
            var entity = await _requestRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.CropVarietyId.HasValue) entity.CropVarietyId = dto.CropVarietyId;
            if (dto.ProcedureTemplateId.HasValue) entity.ProcedureTemplateId = dto.ProcedureTemplateId;
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Objective != null) entity.Objective = dto.Objective;
            if (dto.ExpectedStartDate.HasValue) entity.ExpectedStartDate = dto.ExpectedStartDate;
            if (dto.ExpectedEndDate.HasValue) entity.ExpectedEndDate = dto.ExpectedEndDate;
            if (dto.MonitoringPlan != null) entity.MonitoringPlan = dto.MonitoringPlan;
            await _requestRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update experiment request failed: {ex.Message}"); }
    }

    public async Task<ExperimentRequestResponseDto?> UpdateStatusAsync(Guid id, string status)
    {
        try
        {
            var entity = await _requestRepository.GetByIdAsync(id);
            if (entity == null) return null;
            entity.Status = status;
            await _requestRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update experiment request status failed: {ex.Message}"); }
    }

    public async Task<ExperimentRequestResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _requestRepository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get experiment request failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _requestRepository.GetAllAsync();
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get all experiment requests failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByResearcherAsync(Guid researcherId)
    {
        try
        {
            var entities = await _requestRepository.GetByResearcherAsync(researcherId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiment requests by researcher failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByFarmAsync(Guid farmId)
    {
        try
        {
            var entities = await _requestRepository.GetByFarmAsync(farmId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiment requests by farm failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByStatusAsync(string status)
    {
        try
        {
            var entities = await _requestRepository.GetByStatusAsync(status);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiment requests by status failed: {ex.Message}"); }
    }

    public async Task<RequestReviewResponseDto?> ReviewAsync(Guid requestId, ReviewExperimentRequestDto dto, Guid reviewerId)
    {
        try
        {
            var entity = new M.RequestReview
            {
                RequestId = requestId,
                ReviewerId = reviewerId,
                Comment = dto.Comment,
                Result = dto.Result,
                ReviewedAt = DateTime.UtcNow
            };
            var result = await _reviewRepository.CreateAsync(entity);

            if (dto.Result == "Approved" || dto.Result == "Rejected")
            {
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request != null)
                {
                    request.Status = dto.Result == "Approved" ? "Approved" : "Rejected";
                    await _requestRepository.UpdateAsync(request);
                }
            }

            return new RequestReviewResponseDto
            {
                Id = result.Id,
                ReviewerId = result.ReviewerId,
                Comment = result.Comment,
                Result = result.Result,
                ReviewedAt = result.ReviewedAt
            };
        }
        catch (Exception ex) { throw new Exception($"Review experiment request failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _requestRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _requestRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Delete experiment request failed: {ex.Message}"); }
    }

    private static ExperimentRequestResponseDto MapToResponseDto(M.ExperimentRequest entity)
    {
        return new ExperimentRequestResponseDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Objective = entity.Objective,
            Status = entity.Status,
            ExpectedStartDate = entity.ExpectedStartDate,
            ExpectedEndDate = entity.ExpectedEndDate,
            MonitoringPlan = entity.MonitoringPlan,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            FarmId = entity.FarmId,
            FarmName = entity.Farm?.FarmName,
            ResearcherId = entity.ResearcherId,
            ResearcherName = entity.Researcher?.FullName,
            CropVarietyId = entity.CropVarietyId,
            CropVarietyName = entity.CropVariety?.VarietyName,
            ProcedureTemplateId = entity.ProcedureTemplateId,
            ProcedureTemplateName = entity.ProcedureTemplate?.TemplateName,
            Reviews = entity.RequestReviews?.Select(r => new RequestReviewResponseDto
            {
                Id = r.Id,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer?.FullName,
                Comment = r.Comment,
                Result = r.Result,
                ReviewedAt = r.ReviewedAt
            }).ToList() ?? new()
        };
    }
}