using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Tasks;

public interface IMeasurementRecordService
{
    Task<MeasurementRecordResponseDto> CreateAsync(CreateMeasurementRecordDto dto, Guid measuredBy);
    Task<MeasurementRecordResponseDto> UpdateAsync(Guid id, UpdateMeasurementRecordDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<MeasurementRecordResponseDto> GetByIdAsync(Guid id);
    Task<List<MeasurementRecordResponseDto>> GetByBatchIdAsync(Guid batchId);
    Task<List<MeasurementRecordResponseDto>> GetByExperimentIdAsync(Guid experimentId);
    Task<List<MeasurementRecordResponseDto>> GetByStageIdAsync(Guid stageId);
}
