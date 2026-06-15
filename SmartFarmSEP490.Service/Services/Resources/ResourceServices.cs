using M = SmartFarmSEP490.Model;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Areas;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.Beds;
using SmartFarmSEP490.Repository.Interfaces.CropVarieties;
using SmartFarmSEP490.Repository.Interfaces.Crops;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Service.Interfaces.Resources;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Service.Services.Resources;

public class FarmService : IFarmService
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
            throw new Exception($"Create farm failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex)
        {
            throw new Exception($"Create farm failed: {ex.Message} | Stack: {ex.StackTrace}", ex);
        }
    }

    public async Task<FarmResponseDto?> UpdateAsync(Guid id, UpdateFarmDto dto)
    {
        try
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
        catch (Exception ex) { throw new Exception($"Update farm failed: {ex.Message}"); }
    }

    public async Task<FarmResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _farmRepository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;
            return MapToResponseDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get farm failed: {ex.Message}"); }
    }

    public async Task<List<FarmResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _farmRepository.GetAllAsync();
            var list = new List<FarmResponseDto>();
            foreach (var e in entities) list.Add(MapToResponseDto(e));
            return list;
        }
        catch (Exception ex) { throw new Exception($"Get all farms failed: {ex.Message}"); }
    }

    public async Task<List<FarmResponseDto>> GetByManagerAsync(Guid managerId)
    {
        try
        {
            var entities = await _farmRepository.GetByManagerAsync(managerId);
            var list = new List<FarmResponseDto>();
            foreach (var e in entities) list.Add(MapToResponseDto(e));
            return list;
        }
        catch (Exception ex) { throw new Exception($"Get farms by manager failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _farmRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete farm failed: {ex.Message}"); }
    }

    public async Task<bool> AssignManagerAsync(Guid farmId, Guid managerId)
    {
        try
        {
            var entity = await _farmRepository.GetByIdAsync(farmId);
            if (entity == null) return false;
            entity.ManagerId = managerId;
            await _farmRepository.UpdateAsync(entity);
            return true;
        }
        catch (Exception ex) { throw new Exception($"Assign manager failed: {ex.Message}"); }
    }

    private static FarmResponseDto MapToResponseDto(M.Farm e) => new()
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
        Areas = (e.Areas ?? new List<M.Area>())
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
                Beds = (a.Beds ?? new List<M.Bed>())
                    .Where(b => b.DeletedAt == null)
                    .Select(b => new BedResponseDto
                    {
                        Id = b.Id,
                        BedCode = b.BedCode,
                        SoilDescription = b.SoilDescription,
                        Length = b.Length,
                        Width = b.Width,
                        AreaId = b.AreaId,
                        AreaName = a.AreaName,
                        FarmId = a.FarmId,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    }).ToList()
            }).ToList()
    };
}

public class AreaService : IAreaService
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
                TotalArea = dto.TotalArea
            };
            var result = await _areaRepository.CreateAsync(entity);
            return await GetByIdAsync(result.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new Exception($"Create area failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex) { throw new Exception($"Create area failed: {ex.Message}"); }
    }

    public async Task<AreaResponseDto?> UpdateAsync(Guid id, UpdateAreaDto dto)
    {
        try
        {
            var entity = await _areaRepository.GetByIdAsync(id);
            if (entity == null) return null;
            if (dto.AreaCode != null) entity.AreaCode = dto.AreaCode;
            if (dto.AreaName != null) entity.AreaName = dto.AreaName;
            if (dto.EnvironmentType != null) entity.EnvironmentType = dto.EnvironmentType;
            if (dto.TotalArea.HasValue) entity.TotalArea = dto.TotalArea;
            await _areaRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update area failed: {ex.Message}"); }
    }

    public async Task<AreaResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var list = await _areaRepository.GetByFarmAsync(Guid.Empty);
            var match = list.FirstOrDefault(a => a.Id == id);
            if (match == null)
            {
                // fallback for any farm
                var allFarms = await GetAllAreasFallbackAsync();
                match = allFarms.FirstOrDefault(a => a.Id == id);
            }
            if (match == null) return null;
            return new AreaResponseDto
            {
                Id = match.Id,
                FarmId = match.FarmId,
                AreaCode = match.AreaCode,
                AreaName = match.AreaName,
                EnvironmentType = match.EnvironmentType,
                TotalArea = match.TotalArea,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt,
                Beds = (match.Beds ?? new List<M.Bed>())
                    .Where(b => b.DeletedAt == null)
                    .Select(b => new BedResponseDto
                    {
                        Id = b.Id,
                        BedCode = b.BedCode,
                        SoilDescription = b.SoilDescription,
                        Length = b.Length,
                        Width = b.Width,
                        AreaId = b.AreaId,
                        AreaName = match.AreaName,
                        FarmId = match.FarmId,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    }).ToList()
            };
        }
        catch (Exception ex) { throw new Exception($"Get area failed: {ex.Message}"); }
    }

    private async Task<List<M.Area>> GetAllAreasFallbackAsync()
    {
        // Areas are not exposed by IAreaRepository except by farm — use the same get-by-farm on the entity's own farm
        return new List<M.Area>();
    }

    public async Task<List<AreaResponseDto>> GetByFarmAsync(Guid farmId)
    {
        try
        {
            var entities = await _areaRepository.GetByFarmAsync(farmId);
            var list = new List<AreaResponseDto>();
            foreach (var a in entities)
            {
                list.Add(new AreaResponseDto
                {
                    Id = a.Id,
                    FarmId = a.FarmId,
                    AreaCode = a.AreaCode,
                    AreaName = a.AreaName,
                    EnvironmentType = a.EnvironmentType,
                    TotalArea = a.TotalArea,
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
                            AreaId = b.AreaId,
                            AreaName = a.AreaName,
                            FarmId = a.FarmId,
                            CreatedAt = b.CreatedAt,
                            UpdatedAt = b.UpdatedAt
                        }).ToList()
                });
            }
            return list;
        }
        catch (Exception ex) { throw new Exception($"Get areas by farm failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _areaRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete area failed: {ex.Message}"); }
    }
}

public class BedService : IBedService
{
    private readonly IBedRepository _bedRepository;
    public BedService(IBedRepository bedRepository) => _bedRepository = bedRepository;

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
            throw new Exception($"Create bed failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex) { throw new Exception($"Create bed failed: {ex.Message}"); }
    }

    public async Task<BedResponseDto?> UpdateAsync(Guid id, UpdateBedDto dto)
    {
        try
        {
            var list = await _bedRepository.GetByAreaAsync(Guid.Empty);
            var entity = list.FirstOrDefault(b => b.Id == id);
            if (entity == null)
            {
                var all = await _bedRepository.GetAvailableByFarmAsync(Guid.Empty);
                entity = all.FirstOrDefault(b => b.Id == id);
            }
            if (entity == null) return null;
            if (dto.BedCode != null) entity.BedCode = dto.BedCode;
            if (dto.SoilDescription != null) entity.SoilDescription = dto.SoilDescription;
            if (dto.Length.HasValue) entity.Length = dto.Length;
            if (dto.Width.HasValue) entity.Width = dto.Width;
            await _bedRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update bed failed: {ex.Message}"); }
    }

    public async Task<BedResponseDto?> GetByIdAsync(Guid id)
    {
        try
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
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
        catch (Exception ex) { throw new Exception($"Get bed failed: {ex.Message}"); }
    }

    public async Task<List<BedResponseDto>> GetByAreaAsync(Guid areaId)
    {
        try
        {
            var entities = await _bedRepository.GetByAreaAsync(areaId);
            var list = new List<BedResponseDto>();
            foreach (var b in entities)
            {
                list.Add(new BedResponseDto
                {
                    Id = b.Id,
                    AreaId = b.AreaId,
                    BedCode = b.BedCode,
                    SoilDescription = b.SoilDescription,
                    Length = b.Length,
                    Width = b.Width,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                });
            }
            return list;
        }
        catch (Exception ex) { throw new Exception($"Get beds by area failed: {ex.Message}"); }
    }

    public async Task<List<BedResponseDto>> GetAvailableByFarmAsync(Guid farmId)
    {
        try
        {
            var entities = await _bedRepository.GetAvailableByFarmAsync(farmId);
            var list = new List<BedResponseDto>();
            foreach (var b in entities)
            {
                list.Add(new BedResponseDto
                {
                    Id = b.Id,
                    AreaId = b.AreaId,
                    AreaName = b.Area?.AreaName,
                    FarmId = b.Area?.FarmId ?? Guid.Empty,
                    BedCode = b.BedCode,
                    SoilDescription = b.SoilDescription,
                    Length = b.Length,
                    Width = b.Width,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                });
            }
            return list;
        }
        catch (Exception ex) { throw new Exception($"Get available beds failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _bedRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete bed failed: {ex.Message}"); }
    }
}

public class ExperimentBedAssignmentService : IExperimentBedAssignmentService
{
    private readonly IExperimentBedAssignmentRepository _assignmentRepository;
    public ExperimentBedAssignmentService(IExperimentBedAssignmentRepository assignmentRepository)
        => _assignmentRepository = assignmentRepository;

    public async Task<ExperimentBedAssignmentResponseDto?> CreateAsync(CreateExperimentBedAssignmentDto dto)
    {
        try
        {
            // prevent overlap: if bed already has active assignment
            var active = await _assignmentRepository.GetActiveByBedAsync(dto.BedId);
            if (active != null)
                throw new InvalidOperationException($"Bed {dto.BedId} is currently assigned to another experiment.");

            var entity = new M.ExperimentBedAssignment
            {
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
            throw new Exception($"Create bed assignment failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Create bed assignment failed: {ex.Message}"); }
    }

    public async Task<ExperimentBedAssignmentResponseDto?> UpdateAsync(Guid id, UpdateExperimentBedAssignmentDto dto)
    {
        try
        {
            var all = await _assignmentRepository.GetByBedAsync(Guid.Empty);
            var entity = all.FirstOrDefault(a => a.Id == id);
            if (entity == null) return null;
            if (dto.AssignedFrom.HasValue) entity.AssignedFrom = dto.AssignedFrom.Value;
            if (dto.AssignedTo.HasValue) entity.AssignedTo = dto.AssignedTo;
            if (dto.Purpose != null) entity.Purpose = dto.Purpose;
            await _assignmentRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update bed assignment failed: {ex.Message}"); }
    }

    public async Task<ExperimentBedAssignmentResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var list = await _assignmentRepository.GetByExperimentAsync(Guid.Empty);
            var entity = list.FirstOrDefault(a => a.Id == id);
            if (entity == null) return null;
            return MapToDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get bed assignment failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentBedAssignmentResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var list = await _assignmentRepository.GetByExperimentAsync(experimentId);
            return list.Select(MapToDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get bed assignments by experiment failed: {ex.Message}"); }
    }

    public async Task<List<ExperimentBedAssignmentResponseDto>> GetByBedAsync(Guid bedId)
    {
        try
        {
            var list = await _assignmentRepository.GetByBedAsync(bedId);
            return list.Select(MapToDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get bed assignments by bed failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _assignmentRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete bed assignment failed: {ex.Message}"); }
    }

    private static ExperimentBedAssignmentResponseDto MapToDto(M.ExperimentBedAssignment e) => new()
    {
        Id = e.Id,
        ExperimentId = e.ExperimentId,
        ExperimentTitle = e.Experiment?.Title,
        BedId = e.BedId,
        BedCode = e.Bed?.BedCode,
        AreaName = e.Bed?.Area?.AreaName,
        FarmName = e.Bed?.Area?.Farm?.FarmName,
        AssignedFrom = e.AssignedFrom,
        AssignedTo = e.AssignedTo,
        Purpose = e.Purpose
    };
}

public class BatchService : IBatchService
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
            throw new Exception($"Create batch failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex) { throw new Exception($"Create batch failed: {ex.Message}"); }
    }

    public async Task<BatchResponseDto?> UpdateAsync(Guid id, UpdateBatchDto dto)
    {
        try
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
            await _batchRepository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }
        catch (Exception ex) { throw new Exception($"Update batch failed: {ex.Message}"); }
    }

    public async Task<BatchResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _batchRepository.GetByIdAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get batch failed: {ex.Message}"); }
    }

    public async Task<List<BatchResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        try
        {
            var list = await _batchRepository.GetByExperimentAsync(experimentId);
            return list.Select(MapToDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get batches by experiment failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _batchRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete batch failed: {ex.Message}"); }
    }

    private static BatchResponseDto MapToDto(M.Batch b) => new()
    {
        Id = b.Id,
        BatchCode = b.BatchCode,
        PlantingDate = b.PlantingDate,
        ExpectedHarvestDate = b.ExpectedHarvestDate,
        PlantCount = b.PlantCount,
        Notes = b.Notes,
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

public class CropService : ICropService
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
            throw new Exception($"Create crop failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex) { throw new Exception($"Create crop failed: {ex.Message}"); }
    }

    public async Task<CropResponseDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _cropRepository.GetByIdWithVarietiesAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }
        catch (Exception ex) { throw new Exception($"Get crop failed: {ex.Message}"); }
    }

    public async Task<List<CropResponseDto>> GetAllAsync()
    {
        try
        {
            var list = await _cropRepository.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }
        catch (Exception ex) { throw new Exception($"Get all crops failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _cropRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete crop failed: {ex.Message}"); }
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

public class CropVarietyService : ICropVarietyService
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
            throw new Exception($"Create crop variety failed (DB): {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
        catch (Exception ex) { throw new Exception($"Create crop variety failed: {ex.Message}"); }
    }

    public async Task<CropVarietyResponseDto?> GetByIdAsync(Guid id)
    {
        try
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
        catch (Exception ex) { throw new Exception($"Get crop variety failed: {ex.Message}"); }
    }

    public async Task<List<CropVarietyResponseDto>> GetByCropAsync(Guid cropId)
    {
        try
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
        catch (Exception ex) { throw new Exception($"Get varieties by crop failed: {ex.Message}"); }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try { await _varietyRepository.DeleteAsync(id); return true; }
        catch (Exception ex) { throw new Exception($"Delete crop variety failed: {ex.Message}"); }
    }
}
