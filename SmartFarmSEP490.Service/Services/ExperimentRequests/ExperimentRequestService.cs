using M = SmartFarmSEP490.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Service.Interfaces.ExperimentRequests;

namespace SmartFarmSEP490.Service.Services.ExperimentRequests;

public class ExperimentRequestService : IExperimentRequestService
{
    private readonly IExperimentRequestRepository _requestRepository;
    private readonly IRequestReviewRepository _reviewRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly ILogger<ExperimentRequestService> _logger;

    public ExperimentRequestService(
        IExperimentRequestRepository requestRepository,
        IRequestReviewRepository reviewRepository,
        IFarmRepository farmRepository,
        ILogger<ExperimentRequestService> logger)
    {
        _requestRepository = requestRepository;
        _reviewRepository = reviewRepository;
        _farmRepository = farmRepository;
        _logger = logger;
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
                Status = Enum.Parse<RequestStatus>("Pending")
            };
            var result = await _requestRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException ex)
        {
            var pg = ex.InnerException as Npgsql.PostgresException;
            _logger.LogError(ex,
                "Create experiment request failed. SqlState={SqlState} Detail={Detail} WhereFragment={Hint}",
                pg?.SqlState, pg?.Detail, pg?.Where);
            throw;
        }
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
        catch (DbUpdateException ex)
        {
            var pg = ex.InnerException as Npgsql.PostgresException;
            _logger.LogError(ex,
                "Update experiment request failed. SqlState={SqlState} Detail={Detail}",
                pg?.SqlState, pg?.Detail);
            throw;
        }
    }

    public async Task<ExperimentRequestResponseDto?> UpdateStatusAsync(Guid id, string status)
    {
        try
        {
            var entity = await _requestRepository.GetByIdAsync(id);
            if (entity == null) return null;
            entity.Status = Enum.Parse<RequestStatus>(status);
            await _requestRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (DbUpdateException ex)
        {
            var pg = ex.InnerException as Npgsql.PostgresException;
            _logger.LogError(ex,
                "Update experiment request status failed. SqlState={SqlState}",
                pg?.SqlState);
            throw;
        }
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

    public async Task<List<ExperimentRequestResponseDto>> GetByManagerAsync(Guid managerId, RequestStatus? status)
    {
        try
        {
            var entities = await _requestRepository.GetByManagerAsync(managerId, status);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get experiment requests by manager failed: {ex.Message}"); }
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
            await _reviewRepository.CreateAsync(entity);

            // Map ReviewResult -> RequestStatus for parent request
            var newRequestStatus = dto.Result == ReviewResult.Approved
                ? RequestStatus.Approved
                : RequestStatus.Rejected;
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request != null)
            {
                request.Status = newRequestStatus;
                await _requestRepository.UpdateAsync(request);
            }

            // Reload the review list with full Reviewer (UserRoles -> Role) eagerly loaded
            var reviews = await _reviewRepository.GetByRequestIdAsync(requestId);
            var saved = reviews.FirstOrDefault(r => r.ReviewerId == reviewerId);
            if (saved == null) return null;
            return MapToReviewResponseDto(saved);
        }
        catch (DbUpdateException ex)
        {
            var pg = ex.InnerException as Npgsql.PostgresException;
            _logger.LogError(ex,
                "Review experiment request failed. SqlState={SqlState} Detail={Detail}",
                pg?.SqlState, pg?.Detail);
            throw;
        }
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

    public async Task<ResourceValidationResultDto?> ValidateResourcesAsync(Guid requestId)
    {
        try
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null) return null;

            var resources = await _farmRepository.GetFarmResourceSummaryAsync(request.FarmId);
            if (resources == null) return null;

            bool sufficientBeds = resources.AvailableBeds > 0;
            bool isValid = sufficientBeds;

            string? message;
            if (!sufficientBeds)
                message = $"Farm '{resources.FarmName}' hiện không có beds khả dụng cho thực nghiệm mới (đang có {resources.InUseBeds}/{resources.TotalBeds} beds đang được sử dụng).";
            else
                message = $"Farm '{resources.FarmName}' có {resources.AvailableBeds}/{resources.TotalBeds} beds khả dụng cho thực nghiệm.";

            return new ResourceValidationResultDto
            {
                IsValid = isValid,
                SufficientBeds = sufficientBeds,
                SufficientSensors = resources.TotalSensors > 0,
                Message = message,
                Resources = resources
            };
        }
        catch (Exception ex) { throw new Exception($"Validate resources failed: {ex.Message}"); }
    }

    private static ExperimentRequestResponseDto MapToResponseDto(M.ExperimentRequest entity)
    {
        return new ExperimentRequestResponseDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Objective = entity.Objective,
            Status = entity.Status.ToString(),
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
            Reviews = entity.RequestReviews?.Select(MapToReviewResponseDto).ToList() ?? new()
        };
    }

    private static RequestReviewResponseDto MapToReviewResponseDto(M.RequestReview r)
    {
        return new RequestReviewResponseDto
        {
            Id = r.Id,
            ReviewerId = r.ReviewerId,
            Reviewer = r.Reviewer == null ? null : new ReviewerInfoDto
            {
                Id = r.Reviewer.Id,
                FullName = r.Reviewer.FullName,
                Email = r.Reviewer.Email,
                Phone = r.Reviewer.Phone,
                ProfileDescription = r.Reviewer.ProfileDescription,
                IsActive = r.Reviewer.IsActive,
                CreatedAt = r.Reviewer.CreatedAt,
                Roles = r.Reviewer.UserRoles?
                    .Where(ur => ur.Role != null)
                    .Select(ur => ur.Role.RoleName)
                    .ToList()
            },
            Comment = r.Comment,
            Result = r.Result?.ToString() ?? string.Empty,
            ReviewedAt = r.ReviewedAt
        };
    }
}