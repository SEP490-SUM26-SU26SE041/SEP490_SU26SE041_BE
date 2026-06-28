using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Tasks;

public interface ITaskReportService
{
    Task<TaskReportResponseDto?> CreateAsync(CreateTaskReportDto dto, Guid reporterId);
    Task<TaskReportResponseDto?> UpdateAsync(Guid id, UpdateTaskReportDto dto);
    Task<TaskReportResponseDto?> GetByIdAsync(Guid id);
    Task<List<TaskReportResponseDto>> GetByTaskIdAsync(Guid taskId);
    Task<List<TaskReportResponseDto>> GetByBatchIdAsync(Guid batchId);
}

public interface IMeasurementRecordService
{
    Task<MeasurementRecordResponseDto?> CreateAsync(CreateMeasurementRecordDto dto, Guid measuredBy);
    Task<MeasurementRecordResponseDto?> UpdateAsync(Guid id, UpdateMeasurementRecordDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<List<MeasurementRecordResponseDto>> GetByBatchIdAsync(Guid batchId);
}

public interface ITaskImageService
{
    Task<PlantImageResponseDto?> UploadAsync(UploadTaskImageDto dto, Guid uploadedBy);
    Task<List<PlantImageResponseDto>> GetByTaskReportIdAsync(Guid taskReportId);
    Task<List<PlantImageResponseDto>> GetByBatchIdAsync(Guid batchId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
