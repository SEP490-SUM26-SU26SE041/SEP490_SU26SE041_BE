using Microsoft.AspNetCore.Http;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Commons;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class TaskImageService : ITaskImageService
{
    private readonly IPlantImageRepository _imageRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public TaskImageService(IPlantImageRepository imageRepository, ICloudinaryService cloudinaryService)
    {
        _imageRepository = imageRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async System.Threading.Tasks.Task<PlantImageResponseDto?> UploadAsync(
        IFormFile file,
        Guid experimentId,
        Guid? batchId,
        Guid? taskReportId,
        string? caption,
        DateTime? capturedAt,
        Guid uploadedBy,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return null;

        // Push the actual file bytes to Cloudinary; receive a hosted imageUrl
        var imageUrl = await _cloudinaryService.UploadImageAsync(file, "smartfarm/task-images", ct);

        var image = new PlantImage
        {
            Id = Guid.NewGuid(),
            ExperimentId = experimentId,
            BatchId = batchId,
            TaskReportId = taskReportId,
            ImageUrl = imageUrl,
            Caption = caption,
            UploadedBy = uploadedBy,
            CapturedAt = capturedAt,
            CreatedAt = DateTime.UtcNow
        };

        await _imageRepository.CreateAsync(image);
        return await MapToResponseDto(image);
    }

    public async System.Threading.Tasks.Task<List<PlantImageResponseDto>> GetByTaskReportIdAsync(Guid taskReportId)
    {
        var images = await _imageRepository.GetByTaskReportIdAsync(taskReportId);
        var results = new List<PlantImageResponseDto>();
        foreach (var i in images) results.Add(await MapToResponseDto(i));
        return results;
    }

    public async System.Threading.Tasks.Task<List<PlantImageResponseDto>> GetByBatchIdAsync(Guid batchId)
    {
        var images = await _imageRepository.GetByBatchIdAsync(batchId);
        // fallback - keep existing repo call if it expects taskReportId - we'll route via the right repo method below
        var results = new List<PlantImageResponseDto>();
        foreach (var i in images) results.Add(await MapToResponseDto(i));
        return results;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null) return false;

        await _imageRepository.DeleteAsync(id);
        return true;
    }

    private async System.Threading.Tasks.Task<PlantImageResponseDto> MapToResponseDto(PlantImage image)
    {
        return new PlantImageResponseDto
        {
            Id = image.Id,
            ExperimentId = image.ExperimentId,
            BatchId = image.BatchId,
            BatchCode = image.Batch?.BatchCode,
            TaskReportId = image.TaskReportId,
            ImageUrl = image.ImageUrl,
            Caption = image.Caption,
            UploadedBy = image.UploadedBy,
            UploadedByName = image.UploadedByNavigation?.FullName,
            CapturedAt = image.CapturedAt,
            CreatedAt = image.CreatedAt
        };
    }
}
