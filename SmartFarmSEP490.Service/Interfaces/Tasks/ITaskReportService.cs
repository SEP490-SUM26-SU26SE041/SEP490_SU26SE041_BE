using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Tasks;

public interface ITaskReportService
{
    Task<TaskReportResponseDto?> CreateAsync(CreateTaskReportDto dto, Guid reporterId);
    Task<TaskReportResponseDto?> UpdateAsync(Guid id, UpdateTaskReportDto dto, Guid userId);
    Task<TaskReportResponseDto?> GetByIdAsync(Guid id);
    Task<List<TaskReportResponseDto>> GetByTaskIdAsync(Guid taskId);
    Task<List<TaskReportResponseDto>> GetByBatchIdAsync(Guid batchId);
    Task<bool> DeleteAsync(Guid id);
}

public interface ITaskImageService
{
    Task<PlantImageResponseDto?> UploadAsync(UploadTaskImageDto dto, Guid uploadedBy);
    Task<List<PlantImageResponseDto>> GetByTaskReportIdAsync(Guid taskReportId);
    Task<List<PlantImageResponseDto>> GetByBatchIdAsync(Guid batchId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
