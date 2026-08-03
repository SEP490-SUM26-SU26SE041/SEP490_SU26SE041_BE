using M = SmartFarmSEP490.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Beds;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Service.Interfaces.ExperimentRequests;
using SmartFarmSEP490.Service.Services.Helpers;

namespace SmartFarmSEP490.Service.Services.ExperimentRequests;

public class ExperimentRequestService : IExperimentRequestService
{
    private readonly IExperimentRequestRepository _requestRepository;
    private readonly IRequestReviewRepository _reviewRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IBedRepository _bedRepository;
    private readonly IExperimentBedAssignmentRepository _bedAssignmentRepository;
    private readonly ILogger<ExperimentRequestService> _logger;

    public ExperimentRequestService(
        IExperimentRequestRepository requestRepository,
        IRequestReviewRepository reviewRepository,
        IFarmRepository farmRepository,
        IBedRepository bedRepository,
        IExperimentBedAssignmentRepository bedAssignmentRepository,
        ILogger<ExperimentRequestService> logger)
    {
        _requestRepository = requestRepository;
        _reviewRepository = reviewRepository;
        _farmRepository = farmRepository;
        _bedRepository = bedRepository;
        _bedAssignmentRepository = bedAssignmentRepository;
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay thong tin yeu cau thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _requestRepository.GetAllAsync();
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach yeu cau thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByResearcherAsync(Guid researcherId)
    {
        try
        {
            var entities = await _requestRepository.GetByResearcherAsync(researcherId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay yeu cau theo nha nghien cuu that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByFarmAsync(Guid farmId)
    {
        try
        {
            var entities = await _requestRepository.GetByFarmAsync(farmId);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay yeu cau theo trai that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByStatusAsync(string status)
    {
        try
        {
            var entities = await _requestRepository.GetByStatusAsync(status);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay yeu cau theo trang thai that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentRequestResponseDto>> GetByManagerAsync(Guid managerId, RequestStatus? status)
    {
        try
        {
            var entities = await _requestRepository.GetByManagerAsync(managerId, status);
            return entities.Select(MapToResponseDto).ToList();
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay yeu cau theo quan ly that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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

            var newRequestStatus = dto.Result == ReviewResult.Approved
                ? RequestStatus.Approved
                : RequestStatus.Rejected;

            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request != null)
            {
                request.Status = newRequestStatus;
                await _requestRepository.UpdateAsync(request);

                if (dto.Result == ReviewResult.Approved)
                {
                    await AutoReserveAndRandomizeAsync(requestId, request);
                }
            }

            var reviews = await _reviewRepository.GetByRequestIdAsync(requestId);
            var saved = reviews.FirstOrDefault(r => r.ReviewerId == reviewerId);
            if (saved == null) return null;
            return MapToReviewResponseDto(saved);
        }
        catch (InvalidOperationException) { throw; }
        catch (DbUpdateException ex)
        {
            var pg = ex.InnerException as Npgsql.PostgresException;
            _logger.LogError(ex, "Review experiment request failed. SqlState={SqlState} Detail={Detail}", pg?.SqlState, pg?.Detail);
            throw;
        }
    }

    private async Task AutoReserveAndRandomizeAsync(Guid requestId, M.ExperimentRequest request)
    {
        _logger.LogInformation($"[DEBUG] AutoReserveAndRandomizeAsync START - requestId={requestId}, MonitoringPlan={request.MonitoringPlan}");

        var monitoringPlan = ParseMonitoringPlan(request.MonitoringPlan);
        if (monitoringPlan == null)
        {
            _logger.LogWarning($"[DEBUG] ParseMonitoringPlan returned NULL. Raw JSON: {request.MonitoringPlan}");
        }
        else
        {
            _logger.LogInformation($"[DEBUG] ParseMonitoringPlan SUCCESS - DesignType={monitoringPlan.DesignType}, ReplicationCount={monitoringPlan.ReplicationCount}, Treatments count={monitoringPlan.Treatments?.Count ?? 0}");
        }

        var designType = monitoringPlan?.DesignType ?? Model.Enums.DesignType.Other;
        var replicationCount = monitoringPlan?.ReplicationCount ?? 1;

        List<string>? treatmentNames = null;
        if (monitoringPlan?.Treatments != null && monitoringPlan.Treatments.Count > 0)
        {
            treatmentNames = monitoringPlan.Treatments.Select(t => t.Name).ToList();
        }
        else if (monitoringPlan?.FactorialFactors != null && monitoringPlan.FactorialFactors.Count > 0)
        {
            treatmentNames = RandomizationHelper.GenerateFactorialGroupNames(monitoringPlan.FactorialFactors);
        }
        else
        {
            treatmentNames = RandomizationHelper.GenerateDefaultTreatments(2);
        }

        int expectedGroups = treatmentNames.Count;
        int requiredBeds = replicationCount * expectedGroups;

        _logger.LogInformation($"[DEBUG] expectedGroups={expectedGroups}, replicationCount={replicationCount}, requiredBeds={requiredBeds}");

        var availableBedIds = await _bedAssignmentRepository.GetAvailableBedIdsByFarmAsync(request.FarmId);
        _logger.LogInformation($"[DEBUG] availableBedIds count={availableBedIds.Count}");

        if (availableBedIds.Count < requiredBeds)
            throw new InvalidOperationException($"Khong du beds. Can {requiredBeds} lo, chi co {availableBedIds.Count} lo kha dung.");

        var selectedBedIds = RandomizationHelper.Shuffle(availableBedIds).Take(requiredBeds).ToList();
        _logger.LogInformation($"[DEBUG] selectedBedIds count={selectedBedIds.Count}, expected={requiredBeds}");

        var reservations = selectedBedIds.Select(bedId => new M.ExperimentBedAssignment
        {
            RequestId = requestId,
            ExperimentId = null,
            BedId = bedId,
            Status = AllocationStatus.Reserved,
            AssignedFrom = request.ExpectedStartDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        }).ToList();

        await _bedAssignmentRepository.CreateRangeAsync(reservations);
        _logger.LogInformation($"[DEBUG] AutoReserveAndRandomizeAsync END - created {reservations.Count} reservations");
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private MonitoringPlanDto? ParseMonitoringPlan(string? monitoringPlanJson)
    {
        if (string.IsNullOrWhiteSpace(monitoringPlanJson))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<MonitoringPlanDto>(monitoringPlanJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<BedReservationResponseDto?> GetReservedBedsAsync(Guid requestId)
    {
        var assignments = await _bedAssignmentRepository.GetByRequestAsync(requestId);
        if (assignments.Count == 0) return null;
        return new BedReservationResponseDto
        {
            RequestId = requestId,
            ReservedCount = assignments.Count,
            ReservedBeds = assignments.Select(a => new BedResponseDto
            {
                Id = a.Bed.Id, BedCode = a.Bed.BedCode, SoilDescription = a.Bed.SoilDescription,
                Length = a.Bed.Length, Width = a.Bed.Width, AllocationStatus = a.Status.ToString(),
                AreaId = a.Bed.AreaId, AreaName = a.Bed.Area?.AreaName,
                FarmId = a.Bed.Area?.FarmId ?? Guid.Empty,
                CreatedAt = a.Bed.CreatedAt, UpdatedAt = a.Bed.UpdatedAt
            }).ToList()
        };
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
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa yeu cau thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ResourceValidationResultDto?> ValidateResourcesAsync(Guid requestId, int? replicationCount = null, int? expectedGroups = null)
    {
        try
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null) return null;

            var monitoringPlan = ParseMonitoringPlan(request.MonitoringPlan);
            var actualReplicationCount = replicationCount ?? monitoringPlan?.ReplicationCount ?? 1;

            List<string>? treatmentNames = null;
            if (monitoringPlan?.Treatments != null && monitoringPlan.Treatments.Count > 0)
            {
                treatmentNames = monitoringPlan.Treatments.Select(t => t.Name).ToList();
            }
            else if (monitoringPlan?.FactorialFactors != null && monitoringPlan.FactorialFactors.Count > 0)
            {
                treatmentNames = RandomizationHelper.GenerateFactorialGroupNames(monitoringPlan.FactorialFactors);
            }
            int actualExpectedGroups = expectedGroups ?? treatmentNames?.Count ?? 2;

            var resources = await _farmRepository.GetFarmResourceSummaryAsync(request.FarmId);
            if (resources == null) return null;

            var availableBedIds = await _bedAssignmentRepository.GetAvailableBedIdsByFarmAsync(request.FarmId);
            var availableBeds = await _bedRepository.GetByIdsAsync(availableBedIds);
            var availableBedDtos = availableBeds.Select(b => new BedResponseDto
            {
                Id = b.Id,
                BedCode = b.BedCode,
                SoilDescription = b.SoilDescription,
                Length = b.Length,
                Width = b.Width,
                AllocationStatus = "Available",
                AreaId = b.AreaId,
                AreaName = b.Area?.AreaName,
                FarmId = b.Area?.FarmId ?? Guid.Empty,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            int requiredBeds = actualReplicationCount * actualExpectedGroups;
            bool sufficientBeds = availableBedDtos.Count >= requiredBeds;
            bool isValid = sufficientBeds;

            string? message;
            if (!sufficientBeds)
                message = $"Farm '{resources.FarmName}' khong du beds. Can {requiredBeds} lo (ReplicationCount={actualReplicationCount} x Groups={actualExpectedGroups}), chi co {availableBedDtos.Count} lo kha dung.";
            else
                message = $"Farm '{resources.FarmName}' co {availableBedDtos.Count}/{resources.TotalBeds} beds kha dung. Can {requiredBeds} lo cho thuc nghiem.";

            return new ResourceValidationResultDto
            {
                IsValid = isValid,
                SufficientBeds = sufficientBeds,
                SufficientSensors = resources.TotalSensors > 0,
                RequiredBeds = requiredBeds,
                AvailableBedCount = availableBedDtos.Count,
                Message = message,
                Resources = resources,
                AvailableBeds = availableBedDtos
            };
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Kiem tra tai nguyen that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
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