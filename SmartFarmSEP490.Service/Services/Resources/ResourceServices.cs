using M = SmartFarmSEP490.Model;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Areas;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.Beds;
using SmartFarmSEP490.Repository.Interfaces.CropVarieties;
using SmartFarmSEP490.Repository.Interfaces.Crops;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SvcInterfaces = SmartFarmSEP490.Service.Interfaces.Resources;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Service.Services.Resources;

public class FarmService : SvcInterfaces.IFarmService
{
    private readonly IFarmRepository _farmRepository;
    public FarmService(IFarmRepository farmRepository) => _farmRepository = farmRepository;

    public async Task<FarmResponseDto?> CreateAsync(CreateFarmDto dto, Guid? currentUserId = null)
    {
        try
        {
            var entity = new M.Farm
            {
                FarmCode = dto.FarmCode,
                FarmName = dto.FarmName,
                Location = dto.Location,
                Description = dto.Description
            };
            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
                entity.ManagerId = currentUserId.Value;
            var result = await _farmRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao nong trai that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<FarmResponseDto?> UpdateAsync(Guid id, UpdateFarmDto dto)
    {
        var entity = await _farmRepository.GetByIdAsync(id);
        if (entity == null) return null;
        if (dto.FarmCode != null) entity.FarmCode = dto.FarmCode;
        if (dto.FarmName != null) entity.FarmName = dto.FarmName;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.Description != null) entity.Description = dto.Description;
        await _farmRepository.UpdateAsync(entity);
        return await GetByIdAsync(id);
    }

    public async Task<FarmResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _farmRepository.GetByIdWithDetailsAsync(id);
        if (entity == null) return null;
        return MapToDto(entity);
    }

    public async Task<List<FarmResponseDto>> GetAllAsync()
    {
        var entities = await _farmRepository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<FarmResponseDto>> GetByManagerAsync(Guid managerId)
    {
        var entities = await _farmRepository.GetByManagerAsync(managerId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _farmRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _farmRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> AssignManagerAsync(Guid farmId, Guid managerId)
    {
        var entity = await _farmRepository.GetByIdAsync(farmId);
        if (entity == null) return false;
        entity.ManagerId = managerId;
        await _farmRepository.UpdateAsync(entity);
        return true;
    }

    private static FarmResponseDto MapToDto(M.Farm e) => new()
    {
        Id = e.Id,
        FarmCode = e.FarmCode,
        FarmName = e.FarmName,
        Location = e.Location,
        Description = e.Description,
        ManagerId = e.ManagerId,
        ManagerName = e.Manager?.FullName,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Areas = e.Areas
            .Where(a => a.DeletedAt == null)
            .Select(a => new AreaResponseDto
            {
                Id = a.Id,
                AreaCode = a.AreaCode,
                AreaName = a.AreaName,
                EnvironmentType = a.EnvironmentType,
                TotalArea = a.TotalArea,
                FarmId = a.FarmId,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Status = a.Status.ToString(),
                Beds = (a.Beds ?? new List<M.Bed>())
                    .Where(b => b.DeletedAt == null)
                    .Select(b => new BedResponseDto
                    {
                        Id = b.Id,
                        BedCode = b.BedCode,
                        SoilDescription = b.SoilDescription,
                        Length = b.Length,
                        Width = b.Width,
                        AllocationStatus = null,
                        AreaId = b.AreaId,
                        AreaName = a.AreaName,
                        FarmId = a.FarmId,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    }).ToList()
            }).ToList()
    };
}

public class AreaService : SvcInterfaces.IAreaService
{
    private readonly IAreaRepository _areaRepository;
    public AreaService(IAreaRepository areaRepository) => _areaRepository = areaRepository;

    public async Task<AreaResponseDto?> CreateAsync(CreateAreaDto dto)
    {
        try
        {
            var entity = new M.Area
            {
                FarmId = dto.FarmId,
                AreaCode = dto.AreaCode,
                AreaName = dto.AreaName,
                EnvironmentType = dto.EnvironmentType,
                TotalArea = dto.TotalArea,
                Status = dto.Status
            };
            var result = await _areaRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao khu vuc that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<AreaResponseDto?> UpdateAsync(Guid id, UpdateAreaDto dto)
    {
        var entity = await _areaRepository.GetByIdAsync(id);
        if (entity == null) return null;
        if (dto.AreaCode != null) entity.AreaCode = dto.AreaCode;
        if (dto.AreaName != null) entity.AreaName = dto.AreaName;
        if (dto.EnvironmentType != null) entity.EnvironmentType = dto.EnvironmentType;
        if (dto.TotalArea.HasValue) entity.TotalArea = dto.TotalArea;
        if (dto.Status.HasValue) entity.Status = dto.Status.Value;
        await _areaRepository.UpdateAsync(entity);
        return await GetByIdAsync(id);
    }

    public async Task<AreaResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _areaRepository.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDto(entity);
    }

    public async Task<List<AreaResponseDto>> GetByFarmAsync(Guid farmId)
    {
        var entities = await _areaRepository.GetByFarmAsync(farmId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _areaRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _areaRepository.DeleteAsync(id);
        return true;
    }

    private static AreaResponseDto MapToDto(M.Area a) => new()
    {
        Id = a.Id,
        FarmId = a.FarmId,
        AreaCode = a.AreaCode,
        AreaName = a.AreaName,
        EnvironmentType = a.EnvironmentType,
        TotalArea = a.TotalArea,
        Status = a.Status.ToString(),
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Beds = (a.Beds ?? new List<M.Bed>())
            .Where(b => b.DeletedAt == null)
            .Select(b => new BedResponseDto
            {
                Id = b.Id,
                BedCode = b.BedCode,
                SoilDescription = b.SoilDescription,
                Length = b.Length,
                Width = b.Width,
                AllocationStatus = null,
                AreaId = b.AreaId,
                AreaName = a.AreaName,
                FarmId = a.FarmId,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList()
    };
}

public class BedService : SvcInterfaces.IBedService
{
    private readonly IBedRepository _bedRepository;
    private readonly IExperimentBedAssignmentRepository _bedAssignmentRepository;
    public BedService(IBedRepository bedRepository, IExperimentBedAssignmentRepository bedAssignmentRepository)
    {
        _bedRepository = bedRepository;
        _bedAssignmentRepository = bedAssignmentRepository;
    }

    public async Task<BedResponseDto?> CreateAsync(CreateBedDto dto)
    {
        try
        {
            var entity = new M.Bed
            {
                AreaId = dto.AreaId,
                BedCode = dto.BedCode,
                SoilDescription = dto.SoilDescription,
                Length = dto.Length,
                Width = dto.Width
            };
            var result = await _bedRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao lo that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<BedResponseDto?> UpdateAsync(Guid id, UpdateBedDto dto)
    {
        var entity = await _bedRepository.GetByIdAsync(id);
        if (entity == null) return null;
        if (dto.BedCode != null) entity.BedCode = dto.BedCode;
        if (dto.SoilDescription != null) entity.SoilDescription = dto.SoilDescription;
        if (dto.Length.HasValue) entity.Length = dto.Length;
        if (dto.Width.HasValue) entity.Width = dto.Width;
        await _bedRepository.UpdateAsync(entity);
        return await GetByIdAsync(id);
    }

    public async Task<BedResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _bedRepository.GetByIdAsync(id);
        if (entity == null) return null;
        return new BedResponseDto
        {
            Id = entity.Id,
            AreaId = entity.AreaId,
            AreaName = entity.Area?.AreaName,
            FarmId = entity.Area?.FarmId ?? Guid.Empty,
            BedCode = entity.BedCode,
            SoilDescription = entity.SoilDescription,
            Length = entity.Length,
            Width = entity.Width,
            AllocationStatus = null,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<List<BedResponseDto>> GetByAreaAsync(Guid areaId)
    {
        var entities = await _bedRepository.GetByAreaAsync(areaId);
        return entities.Select(b => new BedResponseDto
        {
            Id = b.Id,
            AreaId = b.AreaId,
            AreaName = b.Area?.AreaName,
            FarmId = b.Area?.FarmId ?? Guid.Empty,
            BedCode = b.BedCode,
            SoilDescription = b.SoilDescription,
            Length = b.Length,
            Width = b.Width,
            AllocationStatus = null,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }).ToList();
    }

    public async Task<List<BedResponseDto>> GetAvailableByFarmAsync(Guid farmId)
    {
        var availableBedIds = await _bedAssignmentRepository.GetAvailableBedIdsByFarmAsync(farmId);
        var beds = await _bedRepository.GetByIdsAsync(availableBedIds);
        return beds.Select(b => new BedResponseDto
        {
            Id = b.Id,
            AreaId = b.AreaId,
            AreaName = b.Area?.AreaName,
            FarmId = b.Area?.FarmId ?? Guid.Empty,
            BedCode = b.BedCode,
            SoilDescription = b.SoilDescription,
            Length = b.Length,
            Width = b.Width,
            AllocationStatus = "Available",
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }).ToList();
    }

    public async Task<List<BedResponseDto>> GetReservedByRequestAsync(Guid requestId)
    {
        var assignments = await _bedAssignmentRepository.GetByRequestAsync(requestId);
        return assignments.Select(a => new BedResponseDto
        {
            Id = a.Bed.Id, BedCode = a.Bed.BedCode, SoilDescription = a.Bed.SoilDescription,
            Length = a.Bed.Length, Width = a.Bed.Width, AllocationStatus = a.Status.ToString(),
            AreaId = a.Bed.AreaId, AreaName = a.Bed.Area?.AreaName,
            FarmId = a.Bed.Area?.FarmId ?? Guid.Empty,
            CreatedAt = a.Bed.CreatedAt, UpdatedAt = a.Bed.UpdatedAt
        }).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _bedRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _bedRepository.DeleteAsync(id);
        return true;
    }
}

public class ExperimentBedAssignmentService : SvcInterfaces.IExperimentBedAssignmentService
{
    private readonly IExperimentBedAssignmentRepository _assignmentRepository;
    public ExperimentBedAssignmentService(IExperimentBedAssignmentRepository assignmentRepository)
        => _assignmentRepository = assignmentRepository;

    public async Task<ExperimentBedAssignmentResponseDto?> CreateAsync(CreateExperimentBedAssignmentDto dto)
    {
        try
        {
            if (dto.RequestId.HasValue && dto.RequestId.Value != Guid.Empty)
            {
                await _assignmentRepository.UpdateOrCreateAssignmentAsync(
                    dto.RequestId.Value, dto.BedId, dto.ExperimentId, dto.AssignedFrom, dto.Purpose);
                var updated = await _assignmentRepository.GetByRequestAsync(dto.RequestId.Value);
                var saved = updated.FirstOrDefault(a => a.BedId == dto.BedId);
                return saved == null ? null : MapToDto(saved);
            }

            var active = await _assignmentRepository.GetActiveByBedAsync(dto.BedId);
            if (active != null)
                throw new InvalidOperationException($"Luong {dto.BedId} dang co phan cong thuc nghiem khac (chua ket thuc).");

            var entity = new M.ExperimentBedAssignment
            {
                RequestId = dto.RequestId,
                ExperimentId = dto.ExperimentId,
                BedId = dto.BedId,
                AssignedFrom = dto.AssignedFrom,
                AssignedTo = dto.AssignedTo,
                Purpose = dto.Purpose
            };
            var result = await _assignmentRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao phan cong luong that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (InvalidOperationException) { throw; }
    }

    public async Task<ExperimentBedAssignmentResponseDto?> UpdateAsync(Guid id, UpdateExperimentBedAssignmentDto dto)
    {
        var entity = await _assignmentRepository.GetByIdAsync(id);
        if (entity == null) return null;
        if (dto.AssignedFrom.HasValue) entity.AssignedFrom = dto.AssignedFrom.Value;
        if (dto.AssignedTo.HasValue) entity.AssignedTo = dto.AssignedTo;
        if (dto.Purpose != null) entity.Purpose = dto.Purpose;
        await _assignmentRepository.UpdateAsync(entity);
        return await GetByIdAsync(id);
    }

    public async Task<ExperimentBedAssignmentResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _assignmentRepository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<ExperimentBedAssignmentResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        var list = await _assignmentRepository.GetByExperimentAsync(experimentId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<ExperimentBedAssignmentResponseDto>> GetByBedAsync(Guid bedId)
    {
        var list = await _assignmentRepository.GetByBedAsync(bedId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _assignmentRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _assignmentRepository.DeleteAsync(id);
        return true;
    }

    private static ExperimentBedAssignmentResponseDto MapToDto(M.ExperimentBedAssignment e) => new()
    {
        Id = e.Id,
        RequestId = e.RequestId,
        ExperimentId = e.ExperimentId,
        ExperimentTitle = e.Experiment?.Title,
        BedId = e.BedId,
        BedCode = e.Bed?.BedCode,
        AllocationStatus = e.Status.ToString(),
        AreaName = e.Bed?.Area?.AreaName,
        FarmName = e.Bed?.Area?.Farm?.FarmName,
        AssignedFrom = e.AssignedFrom,
        AssignedTo = e.AssignedTo,
        Purpose = e.Purpose
    };
}

public class BatchService : SvcInterfaces.IBatchService
{
    private readonly IBatchRepository _batchRepository;
    public BatchService(IBatchRepository batchRepository) => _batchRepository = batchRepository;

    public async Task<BatchResponseDto?> CreateAsync(CreateBatchDto dto)
    {
        try
        {
            var entity = new M.Batch
            {
                ExperimentId = dto.ExperimentId,
                ExperimentBedAssignmentId = dto.ExperimentBedAssignmentId,
                GroupId = dto.GroupId,
                CropVarietyId = dto.CropVarietyId,
                BatchCode = dto.BatchCode,
                PlantingDate = dto.PlantingDate,
                ExpectedHarvestDate = dto.ExpectedHarvestDate,
                PlantCount = dto.PlantCount,
                Notes = dto.Notes
            };
            var result = await _batchRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao lo trong that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<BatchResponseDto?> UpdateAsync(Guid id, UpdateBatchDto dto)
    {
        var entity = await _batchRepository.GetByIdAsync(id);
        if (entity == null) return null;
        if (dto.ExperimentBedAssignmentId.HasValue) entity.ExperimentBedAssignmentId = dto.ExperimentBedAssignmentId;
        if (dto.GroupId.HasValue) entity.GroupId = dto.GroupId;
        if (dto.CropVarietyId.HasValue) entity.CropVarietyId = dto.CropVarietyId;
        if (dto.BatchCode != null) entity.BatchCode = dto.BatchCode;
        if (dto.PlantingDate.HasValue) entity.PlantingDate = dto.PlantingDate;
        if (dto.ExpectedHarvestDate.HasValue) entity.ExpectedHarvestDate = dto.ExpectedHarvestDate;
        if (dto.PlantCount.HasValue) entity.PlantCount = dto.PlantCount;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.Status != null) entity.Status = Enum.Parse<SmartFarmSEP490.Model.Enums.BatchStatus>(dto.Status);
        await _batchRepository.UpdateAsync(entity);
        return await GetByIdAsync(id);
    }

    public async Task<BatchResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _batchRepository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<BatchResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        var list = await _batchRepository.GetByExperimentAsync(experimentId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _batchRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _batchRepository.DeleteAsync(id);
        return true;
    }

    private static BatchResponseDto MapToDto(M.Batch b) => new()
    {
        Id = b.Id,
        BatchCode = b.BatchCode,
        PlantingDate = b.PlantingDate,
        ExpectedHarvestDate = b.ExpectedHarvestDate,
        PlantCount = b.PlantCount,
        Notes = b.Notes,
        Status = null,
        CreatedAt = b.CreatedAt,
        ExperimentId = b.ExperimentId,
        ExperimentTitle = b.Experiment?.Title,
        ExperimentBedAssignmentId = b.ExperimentBedAssignmentId,
        BedCode = b.ExperimentBedAssignment?.Bed?.BedCode,
        AreaName = b.ExperimentBedAssignment?.Bed?.Area?.AreaName,
        FarmName = b.ExperimentBedAssignment?.Bed?.Area?.Farm?.FarmName,
        GroupId = b.GroupId,
        GroupName = b.Group?.GroupName,
        CropVarietyId = b.CropVarietyId,
        CropVarietyName = b.CropVariety?.VarietyName
    };
}

public class CropService : SvcInterfaces.ICropService
{
    private readonly ICropRepository _cropRepository;
    public CropService(ICropRepository cropRepository) => _cropRepository = cropRepository;

    public async Task<CropResponseDto?> CreateAsync(CreateCropDto dto)
    {
        try
        {
            var entity = new M.Crop
            {
                CropName = dto.CropName,
                ScientificName = dto.ScientificName,
                Category = dto.Category,
                Description = dto.Description
            };
            var result = await _cropRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao cay trong that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<CropResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _cropRepository.GetByIdWithVarietiesAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<CropResponseDto>> GetAllAsync()
    {
        var list = await _cropRepository.GetAllAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _cropRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _cropRepository.DeleteAsync(id);
        return true;
    }

    private static CropResponseDto MapToDto(M.Crop c) => new()
    {
        Id = c.Id,
        CropName = c.CropName,
        ScientificName = c.ScientificName,
        Category = c.Category,
        Description = c.Description,
        CreatedAt = c.CreatedAt,
        Varieties = (c.CropVarieties ?? new List<M.CropVariety>())
            .Where(v => v.DeletedAt == null)
            .Select(v => new CropVarietyResponseDto
            {
                Id = v.Id,
                VarietyName = v.VarietyName,
                Origin = v.Origin,
                GrowthDurationDays = v.GrowthDurationDays,
                Description = v.Description,
                CropId = v.CropId,
                CropName = c.CropName,
                CreatedAt = v.CreatedAt
            }).ToList()
    };
}

public class CropVarietyService : SvcInterfaces.ICropVarietyService
{
    private readonly ICropVarietyRepository _varietyRepository;
    public CropVarietyService(ICropVarietyRepository varietyRepository) => _varietyRepository = varietyRepository;

    public async Task<CropVarietyResponseDto?> CreateAsync(CreateCropVarietyDto dto)
    {
        try
        {
            var entity = new M.CropVariety
            {
                CropId = dto.CropId,
                VarietyName = dto.VarietyName,
                Origin = dto.Origin,
                GrowthDurationDays = dto.GrowthDurationDays,
                Description = dto.Description
            };
            var result = await _varietyRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException($"Tao giong that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<CropVarietyResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _varietyRepository.GetByIdAsync(id);
        if (entity == null) return null;
        return new CropVarietyResponseDto
        {
            Id = entity.Id,
            VarietyName = entity.VarietyName,
            Origin = entity.Origin,
            GrowthDurationDays = entity.GrowthDurationDays,
            Description = entity.Description,
            CropId = entity.CropId,
            CropName = entity.Crop?.CropName,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<CropVarietyResponseDto>> GetByCropAsync(Guid cropId)
    {
        var list = await _varietyRepository.GetByCropAsync(cropId);
        return list.Select(v => new CropVarietyResponseDto
        {
            Id = v.Id,
            VarietyName = v.VarietyName,
            Origin = v.Origin,
            GrowthDurationDays = v.GrowthDurationDays,
            Description = v.Description,
            CropId = v.CropId,
            CreatedAt = v.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _varietyRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _varietyRepository.DeleteAsync(id);
        return true;
    }
}
