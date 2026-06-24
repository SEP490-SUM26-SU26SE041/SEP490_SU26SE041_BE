using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Service.Interfaces.ExperimentRequests;

public interface IExperimentRequestService
{
    Task<ExperimentRequestResponseDto?> CreateAsync(CreateExperimentRequestDto dto, Guid researcherId);
    Task<ExperimentRequestResponseDto?> UpdateAsync(Guid id, UpdateExperimentRequestDto dto, Guid researcherId);
    Task<ExperimentRequestResponseDto?> UpdateStatusAsync(Guid id, string status);
    Task<ExperimentRequestResponseDto?> GetByIdAsync(Guid id);
    Task<List<ExperimentRequestResponseDto>> GetAllAsync();
    Task<List<ExperimentRequestResponseDto>> GetByResearcherAsync(Guid researcherId);
    Task<List<ExperimentRequestResponseDto>> GetByFarmAsync(Guid farmId);
    Task<List<ExperimentRequestResponseDto>> GetByStatusAsync(string status);
    Task<List<ExperimentRequestResponseDto>> GetByManagerAsync(Guid managerId, RequestStatus? status);
    Task<RequestReviewResponseDto?> ReviewAsync(Guid requestId, ReviewExperimentRequestDto dto, Guid reviewerId);
    Task<bool> DeleteAsync(Guid id);
    Task<ResourceValidationResultDto?> ValidateResourcesAsync(Guid requestId);
    Task<BedReservationResponseDto?> GetReservedBedsAsync(Guid requestId);
}
