using Microsoft.AspNetCore.Http;

namespace SmartFarmSEP490.Model.DTOs;

public class UploadTaskImageForm
{
    public IFormFile File { get; set; } = default!;
    public Guid ExperimentId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? TaskReportId { get; set; }
    public string? Caption { get; set; }
    public DateTime? CapturedAt { get; set; }

    /// <summary>
    /// (Optional) Chọn AI provider. Nếu null/empty sẽ tự suy ra từ Task type:
    ///  - Observation/Inspection task → mặc định TomatoLeafDiseaseOnnx
    ///  - Pest control task → ArgoPestOnnx
    ///  - Có thể override thủ công từ FE.
    /// </summary>
    public string? AIProvider { get; set; }
}
