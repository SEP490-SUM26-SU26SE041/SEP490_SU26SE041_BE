using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class TaskImageService : ITaskImageService
{
    private readonly IPlantImageRepository _imageRepository;

    public TaskImageService(IPlantImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async System.Threading.Tasks.Task<PlantImageResponseDto?> UploadAsync(UploadTaskImageDto dto, Guid uploadedBy)
    {
        var image = new PlantImage
        {
            Id = Guid.NewGuid(),
            ExperimentId = dto.ExperimentId,
            BatchId = dto.BatchId,
            TaskReportId = dto.TaskReportId,
            ImageUrl = dto.ImageUrl,
            Caption = dto.Caption,
            UploadedBy = uploadedBy,
            CapturedAt = dto.CapturedAt,
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
