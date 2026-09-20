using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.Service.Services.Experiments;

public class ExperimentReportService : IExperimentReportService
{
    private readonly IExperimentReportRepository _reportRepository;
    private readonly IExperimentRepository _experimentRepository;

    public ExperimentReportService(
        IExperimentReportRepository reportRepository,
        IExperimentRepository experimentRepository)
    {
        _reportRepository = reportRepository;
        _experimentRepository = experimentRepository;
    }

    public async Task<ExperimentReportResponseDto?> CreateAsync(Guid experimentId, CreateExperimentReportDto dto, Guid userId)
    {
        try
        {
            var experiment = await _experimentRepository.GetByIdAsync(experimentId);
            if (experiment == null)
                throw new InvalidOperationException($"Khong tim thay thuc nghiem voi ID: {experimentId}");

            if (string.IsNullOrWhiteSpace(dto.ReportType))
                throw new InvalidOperationException("ReportType khong duoc de trong.");
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Title khong duoc de trong.");

            var entity = new M.ExperimentReport
            {
                Id = Guid.NewGuid(),
                ExperimentId = experimentId,
                CreatedBy = userId,
                ReportType = dto.ReportType,
                Title = dto.Title,
                Summary = dto.Summary,
                ResultData = dto.ResultData,
                ExportFormat = dto.ExportFormat,
                FileUrl = dto.FileUrl
            };

            var created = await _reportRepository.AddAsync(entity);
            return MapToResponseDto(created);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Tao bao cao thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentReportResponseDto?> UpdateAsync(Guid id, UpdateExperimentReportDto dto, Guid userId)
    {
        try
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.ReportType != null) entity.ReportType = dto.ReportType;
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Summary != null) entity.Summary = dto.Summary;
            if (dto.ResultData != null) entity.ResultData = dto.ResultData;
            if (dto.ExportFormat != null) entity.ExportFormat = dto.ExportFormat;
            if (dto.FileUrl != null) entity.FileUrl = dto.FileUrl;

            await _reportRepository.UpdateAsync(entity);
            return MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Cap nhat bao cao thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<ExperimentReportResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            return entity == null ? null : MapToResponseDto(entity);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay bao cao thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentReportResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var entities = await _reportRepository.GetByExperimentAsync(experimentId);
            var results = new List<ExperimentReportResponseDto>();
            foreach (var e in entities) results.Add(MapToResponseDto(e));
            return results;
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach bao cao theo thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<List<ExperimentReportResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _reportRepository.GetAllAsync();
            var results = new List<ExperimentReportResponseDto>();
            foreach (var e in entities) results.Add(MapToResponseDto(e));
            return results;
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Lay danh sach bao cao thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            if (entity == null) return false;
            await _reportRepository.DeleteAsync(id);
            return true;
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Xoa bao cao thuc nghiem that bai: {ex.InnerException?.Message ?? ex.Message}", ex); }
    }

    private static ExperimentReportResponseDto MapToResponseDto(M.ExperimentReport entity)
    {
        return new ExperimentReportResponseDto
        {
            Id = entity.Id,
            ExperimentId = entity.ExperimentId,
            ExperimentCode = entity.Experiment?.ExperimentCode,
            ExperimentTitle = entity.Experiment?.Title,
            CreatedBy = entity.CreatedBy,
            CreatedByName = entity.CreatedByNavigation?.FullName,
            ReportType = entity.ReportType,
            Title = entity.Title,
            Summary = entity.Summary,
            ResultData = entity.ResultData,
            ExportFormat = entity.ExportFormat,
            FileUrl = entity.FileUrl,
            CreatedAt = entity.CreatedAt
        };
    }
}
