using System.Text.Json;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class TaskReportService : ITaskReportService
{
    private readonly ITaskReportRepository _reportRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IPlantImageRepository _imageRepository;
    private readonly SmartFarmDbContext _context;

    public TaskReportService(
        ITaskReportRepository reportRepository,
        ITaskRepository taskRepository,
        IPlantImageRepository imageRepository,
        SmartFarmDbContext context)
    {
        _reportRepository = reportRepository;
        _taskRepository = taskRepository;
        _imageRepository = imageRepository;
        _context = context;
    }

    public async System.Threading.Tasks.Task<TaskReportResponseDto?> CreateAsync(CreateTaskReportDto dto, Guid reporterId)
    {
        var task = await _taskRepository.GetByIdAsync(dto.TaskId);
        if (task == null) return null;

        var existingReports = await _reportRepository.GetByTaskIdAsync(dto.TaskId);
        if (existingReports.Any())
            throw new InvalidOperationException("Task nay da co report roi, moi task chi duoc gui 1 report.");

        string? resultDataJson = dto.ResultData != null
            ? JsonSerializer.Serialize(dto.ResultData)
            : null;

        var report = new TaskReport
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            ReporterId = reporterId,
            ReportText = dto.ReportText,
            ResultData = resultDataJson,
            ReportedAt = DateTime.UtcNow
        };

        await _reportRepository.CreateAsync(report);
        return await MapToResponseDto(report);
    }

    public async System.Threading.Tasks.Task<TaskReportResponseDto?> UpdateAsync(Guid id, UpdateTaskReportDto dto, Guid userId)
    {
        var existing = await _reportRepository.GetByIdAsync(id);
        if (existing == null) return null;

        if (dto.ReportText != null) existing.ReportText = dto.ReportText;
        if (dto.ResultData != null) existing.ResultData = JsonSerializer.Serialize(dto.ResultData);

        await _reportRepository.UpdateAsync(existing);
        return await MapToResponseDto(existing);
    }

    public async System.Threading.Tasks.Task<TaskReportResponseDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        return report == null ? null : await MapToResponseDto(report);
    }

    public async System.Threading.Tasks.Task<List<TaskReportResponseDto>> GetByTaskIdAsync(Guid taskId)
    {
        var reports = await _reportRepository.GetByTaskIdAsync(taskId);
        var results = new List<TaskReportResponseDto>();
        foreach (var r in reports) results.Add(await MapToResponseDto(r));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskReportResponseDto>> GetByBatchIdAsync(Guid batchId)
    {
        var reports = await _reportRepository.GetByBatchIdAsync(batchId);
        var results = new List<TaskReportResponseDto>();
        foreach (var r in reports) results.Add(await MapToResponseDto(r));
        return results;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _reportRepository.GetByIdAsync(id);
        if (existing == null) return false;

        await _reportRepository.DeleteAsync(id);
        return true;
    }

    private async System.Threading.Tasks.Task<TaskReportResponseDto> MapToResponseDto(TaskReport report)
    {
        var images = await _imageRepository.GetByTaskReportIdAsync(report.Id);

        object? parsedResult = null;
        if (!string.IsNullOrEmpty(report.ResultData))
        {
            try { parsedResult = JsonSerializer.Deserialize<object>(report.ResultData); }
            catch { parsedResult = report.ResultData; }
        }

        return new TaskReportResponseDto
        {
            Id = report.Id,
            TaskId = report.TaskId,
            TaskTitle = report.Task?.Title,
            TaskType = report.Task?.Type.ToString(),
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter?.FullName,
            ReportText = report.ReportText,
            ResultData = parsedResult,
            ReportedAt = report.ReportedAt,
            Images = images.Select(i => new PlantImageResponseDto
            {
                Id = i.Id,
                ExperimentId = i.ExperimentId,
                BatchId = i.BatchId,
                BatchCode = i.Batch?.BatchCode,
                TaskReportId = i.TaskReportId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption,
                UploadedBy = i.UploadedBy,
                UploadedByName = i.UploadedByNavigation?.FullName,
                CapturedAt = i.CapturedAt,
                CreatedAt = i.CreatedAt
            }).ToList()
        };
    }
}
