using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.API.Helpers;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;
using SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;
using SmartFarmSEP490.Repository.Interfaces.CareSchedules;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/experiments")]
[ApiController]
[Authorize]
public class ExperimentsController : ControllerBase
{
    private readonly IExperimentService _experimentService;
    private readonly IExperimentStageService _stageService;
    private readonly IExperimentGroupService _groupService;
    private readonly IExperimentDesignService _designService;
    private readonly IMeasurementDefinitionService _measurementService;
    private readonly IProcedureTemplateService _templateService;
    private readonly ICareScheduleService _scheduleService;
    private readonly IFarmRepository _farmRepository;
    private readonly IExperimentRepository _experimentRepository;
    private readonly IExperimentStageRepository _stageRepository;
    private readonly IExperimentGroupRepository _groupRepository;
    private readonly IMeasurementDefinitionRepository _measurementRepository;
    private readonly ICareScheduleRepository _careScheduleRepository;

    public ExperimentsController(
        IExperimentService experimentService,
        IExperimentStageService stageService,
        IExperimentGroupService groupService,
        IExperimentDesignService designService,
        IMeasurementDefinitionService measurementService,
        IProcedureTemplateService templateService,
        ICareScheduleService scheduleService,
        IFarmRepository farmRepository,
        IExperimentRepository experimentRepository,
        IExperimentStageRepository stageRepository,
        IExperimentGroupRepository groupRepository,
        IMeasurementDefinitionRepository measurementRepository,
        ICareScheduleRepository careScheduleRepository)
    {
        _experimentService = experimentService;
        _stageService = stageService;
        _groupService = groupService;
        _designService = designService;
        _measurementService = measurementService;
        _templateService = templateService;
        _scheduleService = scheduleService;
        _farmRepository = farmRepository;
        _experimentRepository = experimentRepository;
        _stageRepository = stageRepository;
        _groupRepository = groupRepository;
        _measurementRepository = measurementRepository;
        _careScheduleRepository = careScheduleRepository;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? GetRole() => User.FindFirstValue(ClaimTypes.Role);
    private bool IsResearcher() => GetRole() == "Researcher";
    private bool IsManager() => GetRole() == "Manager";
    private bool IsManagerOrResearcher() => IsManager() || IsResearcher();

    private async Task<bool> IsExperimentOwnerAsync(Guid experimentId)
    {
        var exp = await _experimentRepository.GetByIdAsync(experimentId);
        return exp != null && exp.ResearcherId == GetUserId();
    }

    private async Task<bool> CanManageExperimentAsync(Guid experimentId)
    {
        if (IsManager()) return false;
        var exp = await _experimentRepository.GetByIdAsync(experimentId);
        return exp != null && exp.ResearcherId == GetUserId();
    }

    private async Task<bool> CanAccessExperimentAsync(Guid experimentId)
    {
        if (IsManager())
        {
            var exp = await _experimentRepository.GetByIdAsync(experimentId);
            if (exp == null) return false;
            var farm = await _farmRepository.GetByIdAsync(exp.FarmId);
            return farm != null && farm.ManagerId == GetUserId();
        }
        if (IsResearcher())
        {
            var exp = await _experimentRepository.GetByIdAsync(experimentId);
            return exp != null && exp.ResearcherId == GetUserId();
        }
        return false;
    }

    // ========== Experiments ==========

    [HttpPost]
    public async Task<IActionResult> CreateExperiment([FromBody] CreateExperimentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!IsResearcher()) return Forbid();
        try
        {
            var result = await _experimentService.CreateAsync(dto, GetUserId());
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao thuc nghiem." })
                : StatusCode(201, ApiResponse<ExperimentResponseDto>.Created(result!, "Tao thuc nghiem thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPost("from-request/{requestId:guid}")]
    public async Task<IActionResult> CreateExperimentFromRequest(Guid requestId)
    {
        if (!IsResearcher()) return Forbid();
        try
        {
            var result = await _experimentService.CreateFromRequestAsync(requestId, GetUserId());
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao thuc nghiem." })
                : StatusCode(201, ApiResponse<ExperimentResponseDto>.Created(result, "Tao thuc nghiem tu yeu cau thanh cong. Cac lo da duoc chuyen sang trang thai Assigned."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetExperimentById(Guid id)
    {
        var result = await _experimentService.GetByIdAsync(id);
        if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Khong tim thay thuc nghiem." });

        if (IsResearcher() && result.ResearcherId == GetUserId()) return Ok(ApiResponse<ExperimentResponseDto>.Ok(result));
        if (IsManager())
        {
            var farm = await _farmRepository.GetByIdAsync(result.FarmId);
            if (farm != null && farm.ManagerId == GetUserId()) return Ok(ApiResponse<ExperimentResponseDto>.Ok(result));
        }
        return Forbid();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllExperiments([FromQuery] Guid? researcherId, [FromQuery] Guid? farmId)
    {
        if (IsResearcher())
        {
            var userId = GetUserId();
            return Ok(ApiResponse<List<ExperimentResponseDto>>.Ok(await _experimentService.GetByResearcherAsync(userId)));
        }
        if (IsManager())
        {
            var userId = GetUserId();
            var myFarms = await _farmRepository.GetByManagerAsync(userId);
            var myFarmIds = myFarms.Select(f => f.Id).ToHashSet();

            if (farmId.HasValue)
            {
                if (!myFarmIds.Contains(farmId.Value)) return Ok(ApiResponse<List<ExperimentResponseDto>>.Ok(new List<ExperimentResponseDto>()));
                return Ok(ApiResponse<List<ExperimentResponseDto>>.Ok(await _experimentService.GetByFarmAsync(farmId.Value)));
            }

            var results = new List<ExperimentResponseDto>();
            foreach (var fid in myFarmIds)
                results.AddRange(await _experimentService.GetByFarmAsync(fid));
            return Ok(ApiResponse<List<ExperimentResponseDto>>.Ok(results));
        }
        return Forbid();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExperiment(Guid id, [FromBody] UpdateExperimentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!await IsExperimentOwnerAsync(id)) return Forbid();
        try
        {
            var result = await _experimentService.UpdateAsync(id, dto, GetUserId());
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay thuc nghiem." })
                : Ok(ApiResponse<ExperimentResponseDto>.Ok(result, "Cap nhat thuc nghiem thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateExperimentStatusDto dto)
    {
        if (!await IsExperimentOwnerAsync(id)) return Forbid();
        try
        {
            var result = await _experimentService.UpdateStatusAsync(id, dto.Status, GetUserId());
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay thuc nghiem." })
                : Ok(ApiResponse<ExperimentResponseDto>.Ok(result, "Cap nhat trang thai thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExperiment(Guid id)
    {
        if (!await IsExperimentOwnerAsync(id)) return Forbid();
        await _experimentService.DeleteAsync(id);
        return Ok(new ApiResponse { Success = true, Message = "Xoa thuc nghiem thanh cong." });
    }

    // ========== Experiment Stages ==========

    [HttpPost("{experimentId:guid}/stages")]
    public async Task<IActionResult> CreateStage(Guid experimentId, [FromBody] CreateExperimentStageDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _stageService.CreateAsync(experimentId, dto);
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao giai doan." })
                : StatusCode(201, ApiResponse<ExperimentStageResponseDto>.Created(result, "Tao giai doan thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("{experimentId:guid}/stages/{id:guid}")]
    public async Task<IActionResult> GetStageById(Guid experimentId, Guid id)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        var list = await _stageService.GetByExperimentAsync(experimentId);
        var result = list.FirstOrDefault(s => s.Id == id);
        return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay giai doan." }) : Ok(ApiResponse<ExperimentStageResponseDto>.Ok(result));
    }

    [HttpGet("{experimentId:guid}/stages")]
    public async Task<IActionResult> GetStagesByExperiment(Guid experimentId)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        return Ok(ApiResponse<List<ExperimentStageResponseDto>>.Ok(await _stageService.GetByExperimentAsync(experimentId)));
    }

    [HttpPut("stages/{id:guid}")]
    public async Task<IActionResult> UpdateStage(Guid id, [FromBody] UpdateExperimentStageDto dto)
    {
        var stage = await _stageRepository.GetByIdAsync(id);
        if (stage == null) return NotFound(new ApiResponse { Success = false, Message = "Khong tim thay giai doan." });
        if (!await CanManageExperimentAsync(stage.ExperimentId)) return Forbid();
        try
        {
            var result = await _stageService.UpdateAsync(id, dto);
            return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong the cap nhat giai doan." }) : Ok(ApiResponse<ExperimentStageResponseDto>.Ok(result, "Cap nhat giai doan thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("stages/{id:guid}")]
    public async Task<IActionResult> DeleteStage(Guid id)
    {
        var stage = await _stageRepository.GetByIdAsync(id);
        if (stage != null && !await CanManageExperimentAsync(stage.ExperimentId)) return Forbid();
        await _stageService.DeleteAsync(id);
        return Ok(new ApiResponse { Success = true, Message = "Xoa giai doan thanh cong." });
    }

    // ========== Experiment Groups ==========

    [HttpPost("{experimentId:guid}/groups")]
    public async Task<IActionResult> CreateGroup(Guid experimentId, [FromBody] CreateExperimentGroupDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _groupService.CreateAsync(experimentId, dto);
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao nhom." })
                : StatusCode(201, ApiResponse<ExperimentGroupResponseDto>.Created(result, "Tao nhom thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("{experimentId:guid}/groups")]
    public async Task<IActionResult> GetGroupsByExperiment(Guid experimentId)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        return Ok(ApiResponse<List<ExperimentGroupResponseDto>>.Ok(await _groupService.GetByExperimentAsync(experimentId)));
    }

    [HttpGet("{experimentId:guid}/groups/{id:guid}")]
    public async Task<IActionResult> GetGroupById(Guid experimentId, Guid id)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        var list = await _groupService.GetByExperimentAsync(experimentId);
        var result = list.FirstOrDefault(g => g.Id == id);
        return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay nhom." }) : Ok(ApiResponse<ExperimentGroupResponseDto>.Ok(result));
    }

    [HttpPut("groups/{id:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateExperimentGroupDto dto)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group == null) return NotFound(new ApiResponse { Success = false, Message = "Khong tim thay nhom." });
        if (!await CanManageExperimentAsync(group.ExperimentId)) return Forbid();
        try
        {
            var result = await _groupService.UpdateAsync(id, dto);
            return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong the cap nhat nhom." }) : Ok(ApiResponse<ExperimentGroupResponseDto>.Ok(result, "Cap nhat nhom thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("groups/{id:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var group = await _groupRepository.GetByIdAsync(id);
        if (group != null && !await CanManageExperimentAsync(group.ExperimentId)) return Forbid();
        await _groupService.DeleteAsync(id);
        return Ok(new ApiResponse { Success = true, Message = "Xoa nhom thanh cong." });
    }

    // ========== Randomization & Auto-Setup ==========

    [HttpPost("{experimentId:guid}/randomize-beds")]
    public async Task<IActionResult> RandomizeBeds(Guid experimentId)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _experimentService.RandomizeBedsAsync(experimentId);
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay thuc nghiem." })
                : Ok(ApiResponse<RandomizationResultDto>.Ok(result, "Randomize beds thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPut("{experimentId:guid}/groups/supplement")]
    public async Task<IActionResult> SupplementGroups(Guid experimentId, [FromBody] SupplementGroupsDto dto)
    {
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            dto.ExperimentId = experimentId;
            var result = await _experimentService.SupplementGroupsAsync(dto);
            return Ok(ApiResponse<List<ExperimentGroupResponseDto>>.Ok(result, "Cap nhat/bosung nhom thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPost("{experimentId:guid}/auto-setup")]
    public async Task<IActionResult> AutoSetup(Guid experimentId)
    {
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            await _experimentService.AutoSetupExperimentStructureAsync(experimentId);
            var updated = await _experimentService.GetByIdAsync(experimentId);
            return Ok(ApiResponse<ExperimentResponseDto>.Ok(updated!, "Tu dong tao cau truc thuc nghiem (Design, Groups, Batches) thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    // ========== Experiment Design ==========

    [HttpPost("{experimentId:guid}/design")]
    public async Task<IActionResult> CreateDesign(Guid experimentId, [FromBody] CreateExperimentDesignDto dto)
    {
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _designService.CreateAsync(experimentId, dto);
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao thiet ke." })
                : StatusCode(201, ApiResponse<ExperimentDesignResponseDto>.Created(result, "Tao thiet ke thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("{experimentId:guid}/design")]
    public async Task<IActionResult> GetDesignByExperiment(Guid experimentId)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        var result = await _designService.GetByExperimentAsync(experimentId);
        return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay thiet ke." }) : Ok(ApiResponse<ExperimentDesignResponseDto>.Ok(result));
    }

    [HttpPut("{experimentId:guid}/design")]
    public async Task<IActionResult> UpdateDesign(Guid experimentId, [FromBody] UpdateExperimentDesignDto dto)
    {
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _designService.UpdateAsync(experimentId, dto);
            return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong the cap nhat thiet ke." }) : Ok(ApiResponse<ExperimentDesignResponseDto>.Ok(result, "Cap nhat thiet ke thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("{experimentId:guid}/design")]
    public async Task<IActionResult> DeleteDesign(Guid experimentId)
    {
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        await _designService.DeleteAsync(experimentId);
        return Ok(new ApiResponse { Success = true, Message = "Xoa thiet ke thanh cong." });
    }

    // ========== Measurement Definitions ==========

    [HttpPost("{experimentId:guid}/measurements")]
    public async Task<IActionResult> CreateMeasurement(Guid experimentId, [FromBody] CreateMeasurementDefinitionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _measurementService.CreateAsync(experimentId, dto);
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao chi so do luong." })
                : StatusCode(201, ApiResponse<MeasurementDefinitionResponseDto>.Created(result, "Tao chi so do luong thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("{experimentId:guid}/measurements")]
    public async Task<IActionResult> GetMeasurementsByExperiment(Guid experimentId)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        return Ok(ApiResponse<List<MeasurementDefinitionResponseDto>>.Ok(await _measurementService.GetByExperimentAsync(experimentId)));
    }

    [HttpPut("measurements/{id:guid}")]
    public async Task<IActionResult> UpdateMeasurement(Guid id, [FromBody] UpdateMeasurementDefinitionDto dto)
    {
        var m = await _measurementRepository.GetByIdAsync(id);
        if (m == null) return NotFound(new ApiResponse { Success = false, Message = "Khong tim thay chi so do luong." });
        if (!await CanManageExperimentAsync(m.ExperimentId)) return Forbid();
        try
        {
            var result = await _measurementService.UpdateAsync(id, dto);
            return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong the cap nhat chi so do luong." }) : Ok(ApiResponse<MeasurementDefinitionResponseDto>.Ok(result, "Cap nhat chi so do luong thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("measurements/{id:guid}")]
    public async Task<IActionResult> DeleteMeasurement(Guid id)
    {
        var m = await _measurementRepository.GetByIdAsync(id);
        if (m != null && !await CanManageExperimentAsync(m.ExperimentId)) return Forbid();
        await _measurementService.DeleteAsync(id);
        return Ok(new ApiResponse { Success = true, Message = "Xoa chi so do luong thanh cong." });
    }

    private async Task<IActionResult> GetMeasurementById(Guid experimentId, Guid id)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        var list = await _measurementService.GetByExperimentAsync(experimentId);
        var result = list.FirstOrDefault(m => m.Id == id);
        return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay chi so do luong." }) : Ok(ApiResponse<MeasurementDefinitionResponseDto>.Ok(result));
    }

    // ========== Procedure Templates ==========

    [HttpPost("procedure-templates")]
    public async Task<IActionResult> CreateProcedureTemplate([FromBody] CreateProcedureTemplateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!IsResearcher()) return Forbid();
        try
        {
            var result = await _templateService.CreateAsync(dto, GetUserId());
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao mau quy trinh." })
                : StatusCode(201, ApiResponse<ProcedureTemplateResponseDto>.Created(result, "Tao mau quy trinh thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("procedure-templates")]
    public async Task<IActionResult> GetAllProcedureTemplates([FromQuery] Guid? cropVarietyId)
    {
        if (!IsManagerOrResearcher()) return Forbid();
        var result = cropVarietyId.HasValue
            ? await _templateService.GetByCropVarietyAsync(cropVarietyId.Value)
            : await _templateService.GetAllAsync();
        return Ok(ApiResponse<List<ProcedureTemplateResponseDto>>.Ok(result));
    }

    [HttpGet("procedure-templates/{id:guid}")]
    public async Task<IActionResult> GetProcedureTemplateById(Guid id)
    {
        if (!IsManagerOrResearcher()) return Forbid();
        var result = await _templateService.GetByIdAsync(id);
        return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay mau quy trinh." }) : Ok(ApiResponse<ProcedureTemplateResponseDto>.Ok(result));
    }

    [HttpDelete("procedure-templates/{id:guid}")]
    public async Task<IActionResult> DeleteProcedureTemplate(Guid id)
    {
        if (!IsResearcher()) return Forbid();
        await _templateService.DeleteAsync(id);
        return Ok(new ApiResponse { Success = true, Message = "Xoa mau quy trinh thanh cong." });
    }

    // ========== Care Schedules ==========

    [HttpPost("{experimentId:guid}/schedules")]
    public async Task<IActionResult> CreateSchedule(Guid experimentId, [FromBody] CreateCareScheduleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Du lieu khong hop le." });
        if (!await CanManageExperimentAsync(experimentId)) return Forbid();
        try
        {
            var result = await _scheduleService.CreateAsync(experimentId, dto);
            return result == null
                ? NotFound(new ApiResponse { Success = false, Message = "Khong the tao lich cham soc." })
                : StatusCode(201, ApiResponse<CareScheduleResponseDto>.Created(result, "Tao lich cham soc thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("{experimentId:guid}/schedules")]
    public async Task<IActionResult> GetSchedulesByExperiment(Guid experimentId)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        return Ok(ApiResponse<List<CareScheduleResponseDto>>.Ok(await _scheduleService.GetByExperimentAsync(experimentId)));
    }

    [HttpPut("schedules/{id:guid}")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateCareScheduleDto dto)
    {
        var schedule = await _careScheduleRepository.GetByIdAsync(id);
        if (schedule == null) return NotFound(new ApiResponse { Success = false, Message = "Khong tim thay lich cham soc." });
        if (!await CanManageExperimentAsync(schedule.ExperimentId)) return Forbid();
        try
        {
            var result = await _scheduleService.UpdateAsync(id, dto);
            return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong the cap nhat lich cham soc." }) : Ok(ApiResponse<CareScheduleResponseDto>.Ok(result, "Cap nhat lich cham soc thanh cong."));
        }
        catch (InvalidOperationException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("schedules/{id:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        var schedule = await _careScheduleRepository.GetByIdAsync(id);
        if (schedule != null && !await CanManageExperimentAsync(schedule.ExperimentId)) return Forbid();
        await _scheduleService.DeleteAsync(id);
        return Ok(new ApiResponse { Success = true, Message = "Xoa lich cham soc thanh cong." });
    }

    private async Task<IActionResult> GetScheduleById(Guid experimentId, Guid id)
    {
        if (!await CanAccessExperimentAsync(experimentId)) return Forbid();
        var list = await _scheduleService.GetByExperimentAsync(experimentId);
        var result = list.FirstOrDefault(s => s.Id == id);
        return result == null ? NotFound(new ApiResponse { Success = false, Message = "Khong tim thay lich cham soc." }) : Ok(ApiResponse<CareScheduleResponseDto>.Ok(result));
    }
}
