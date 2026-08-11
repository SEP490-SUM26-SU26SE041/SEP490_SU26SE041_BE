using Microsoft.AspNetCore.Http;

namespace SmartFarmSEP490.API.Dtos;

public class UploadTaskImageForm
{
    public IFormFile File { get; set; } = default!;
    public Guid ExperimentId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? TaskReportId { get; set; }
    public string? Caption { get; set; }
    public DateTime? CapturedAt { get; set; }
}
