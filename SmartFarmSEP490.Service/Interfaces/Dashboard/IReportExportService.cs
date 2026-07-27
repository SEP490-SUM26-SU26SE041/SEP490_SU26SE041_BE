using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Dashboard;

public interface IReportExportService
{
    // T27: Report Export
    Task<ExportReportResultDto> GenerateReportAsync(ExportReportRequestDto request, Guid userId);
    Task<List<ExperimentReportDto>> GetExperimentReportsAsync(Guid experimentId);
    Task<ExperimentReportDto?> GetReportByIdAsync(Guid reportId);
    Task<bool> DeleteReportAsync(Guid reportId);
}
