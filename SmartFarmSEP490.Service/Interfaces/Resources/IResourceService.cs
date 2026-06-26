using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Resources;

public interface IFarmService
{
    Task<FarmResponseDto?> CreateAsync(CreateFarmDto dto, Guid? currentUserId = null);
    Task<FarmResponseDto?> UpdateAsync(Guid id, UpdateFarmDto dto);
    Task<FarmResponseDto?> GetByIdAsync(Guid id);
    Task<List<FarmResponseDto>> GetAllAsync();
    Task<List<FarmResponseDto>> GetByManagerAsync(Guid managerId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AssignManagerAsync(Guid farmId, Guid managerId);
}

public interface IAreaService
{
    Task<AreaResponseDto?> CreateAsync(CreateAreaDto dto);
    Task<AreaResponseDto?> UpdateAsync(Guid id, UpdateAreaDto dto);
    Task<AreaResponseDto?> GetByIdAsync(Guid id);
    Task<List<AreaResponseDto>> GetByFarmAsync(Guid farmId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IBedService
{
    Task<BedResponseDto?> CreateAsync(CreateBedDto dto);
    Task<BedResponseDto?> UpdateAsync(Guid id, UpdateBedDto dto);
    Task<BedResponseDto?> GetByIdAsync(Guid id);
    Task<List<BedResponseDto>> GetByAreaAsync(Guid areaId);
    Task<List<BedResponseDto>> GetAvailableByFarmAsync(Guid farmId);
    Task<List<BedResponseDto>> GetReservedByRequestAsync(Guid requestId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IExperimentBedAssignmentService
{
    Task<ExperimentBedAssignmentResponseDto?> CreateAsync(CreateExperimentBedAssignmentDto dto);
    Task<ExperimentBedAssignmentResponseDto?> UpdateAsync(Guid id, UpdateExperimentBedAssignmentDto dto);
    Task<ExperimentBedAssignmentResponseDto?> GetByIdAsync(Guid id);
    Task<List<ExperimentBedAssignmentResponseDto>> GetByExperimentAsync(Guid experimentId);
    Task<List<ExperimentBedAssignmentResponseDto>> GetByBedAsync(Guid bedId);
    Task<bool> DeleteAsync(Guid id);
}

public interface IBatchService
{
    Task<BatchResponseDto?> CreateAsync(CreateBatchDto dto);
    Task<BatchResponseDto?> UpdateAsync(Guid id, UpdateBatchDto dto);
    Task<BatchResponseDto?> GetByIdAsync(Guid id);
    Task<List<BatchResponseDto>> GetByExperimentAsync(Guid experimentId);
    Task<bool> DeleteAsync(Guid id);
}

public interface ICropService
{
    Task<CropResponseDto?> CreateAsync(CreateCropDto dto);
    Task<CropResponseDto?> GetByIdAsync(Guid id);
    Task<List<CropResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(Guid id);
}

public interface ICropVarietyService
{
    Task<CropVarietyResponseDto?> CreateAsync(CreateCropVarietyDto dto);
    Task<CropVarietyResponseDto?> GetByIdAsync(Guid id);
    Task<List<CropVarietyResponseDto>> GetByCropAsync(Guid cropId);
    Task<bool> DeleteAsync(Guid id);
}
