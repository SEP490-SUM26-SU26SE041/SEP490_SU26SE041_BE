using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using SmartFarmSEP490.Service.Interfaces.ExperimentRequests;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/experiment-requests")]
[ApiController]
[Authorize]
public class ExperimentRequestsController : ControllerBase
{
    private readonly IExperimentRequestService _service;
    private readonly IFarmRepository _farmRepository;

    public ExperimentRequestsController(IExperimentRequestService service, IFarmRepository farmRepository)
    {
        _service = service;
        _farmRepository = farmRepository;
    }

    private Guid GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    private string? GetRole() => User.FindFirstValue(ClaimTypes.Role);

    // ============ Researcher: create request ============

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExperimentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Du lieu khong hop le." });

        var role = GetRole();
        if (role != "Researcher")
            return StatusCode(403, new { message = "Chi nha nghien cuu moi duoc tao yeu cau thuc nghiem." });

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized(new { message = "Khong the xac dinh nguoi dung." });

        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
            return BadRequest(new { message = $"Trai voi ID: {dto.FarmId} khong ton tai." });
        if (farm.ManagerId == null)
            return BadRequest(new { message = "Trai chua co quan ly. Khong the gui yeu cau." });

        var result = await _service.CreateAsync(dto, userId);
        if (result == null)
            return StatusCode(500, new { message = "Tao yeu cau that bai." });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, message = "Tao yeu cau thuc nghiem thanh cong.", data = result });
    }

    // ============ Visibility rules ============

    // Researcher: chỉ xem request của chính mình
    // Manager: chỉ xem request gửi đến farm do mình quản lý
    private async Task<IActionResult?> EnsureViewerAccessAsync(ExperimentRequestResponseDto request)
    {
        var userId = GetUserId();
        var role = GetRole();

        if (role == "Researcher" && request.ResearcherId == userId) return null;
        if (role == "Manager")
        {
            var farm = await _farmRepository.GetByIdAsync(request.FarmId);
            if (farm != null && farm.ManagerId == userId) return null;
        }
        return StatusCode(403, new { message = "Ban khong co quyen xem yeu cau nay." });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        var deny = await EnsureViewerAccessAsync(result);
        return deny ?? Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? researcherId, [FromQuery] Guid? farmId, [FromQuery] string? status)
    {
        var userId = GetUserId();
        var role = GetRole();

        if (role != "Researcher" && role != "Manager")
            return StatusCode(403, new { message = "Chi nha nghien cuu va quan ly moi duoc xem danh sach yeu cau." });

        if (role == "Researcher")
        {
            var mine = await _service.GetByResearcherAsync(userId);
            if (!string.IsNullOrEmpty(status))
                return Ok(mine.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)));
            return Ok(mine);
        }

        var myFarms = await _farmRepository.GetByManagerAsync(userId);
        var myFarmIds = myFarms.Select(f => f.Id).ToHashSet();
        if (myFarmIds.Count == 0) return Ok(new List<ExperimentRequestResponseDto>());

        var fromQuery = await _service.GetByFarmAsync(farmId ?? Guid.Empty);
        var list = farmId.HasValue
            ? (myFarmIds.Contains(farmId.Value) ? fromQuery : new List<ExperimentRequestResponseDto>())
            : new List<ExperimentRequestResponseDto>();

        if (!farmId.HasValue)
        {
            foreach (var fid in myFarmIds)
            {
                list.AddRange(await _service.GetByFarmAsync(fid));
            }
        }

        if (!string.IsNullOrEmpty(status))
            list = list.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(list.OrderByDescending(r => r.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExperimentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Du lieu khong hop le." });

        var userId = GetUserId();
        var role = GetRole();
        if (role != "Researcher") return StatusCode(403, new { message = "Chi nha nghien cuu moi duoc cap nhat." });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = "Khong tim thay yeu cau." });
        if (existing.ResearcherId != userId) return StatusCode(403, new { message = "Day khong phai yeu cau cua ban." });
        if (!string.Equals(existing.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Chi yeu cau dang cho (Pending) moi duoc phep cap nhat." });

        var result = await _service.UpdateAsync(id, dto, userId);
        return result == null ? NotFound() : Ok(new { success = true, message = "Cap nhat yeu cau thanh cong.", data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var role = GetRole();
        if (role != "Researcher") return StatusCode(403, new { message = "Chi nha nghien cuu moi duoc xoa." });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = "Khong tim thay yeu cau." });
        if (existing.ResearcherId != userId) return StatusCode(403, new { message = "Day khong phai yeu cau cua ban." });

        await _service.DeleteAsync(id);
        return Ok(new { success = true, message = "Xoa yeu cau thanh cong." });
    }

    [HttpGet("{id:guid}/resource-summary")]
    public async Task<IActionResult> GetResourceSummary(
        Guid id,
        [FromQuery] int? replicationCount = null,
        [FromQuery] int? expectedGroups = null)
    {
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { message = "Chi quan ly moi duoc xem tom tat tai nguyen." });

        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound(new { message = "Khong tim thay yeu cau." });

        var farm = await _farmRepository.GetByIdAsync(request.FarmId);
        if (farm == null) return NotFound(new { message = "Trai khong ton tai." });
        if (farm.ManagerId != GetUserId())
            return StatusCode(403, new { message = "Ban khong phai la quan ly trai nay." });

        var result = await _service.ValidateResourcesAsync(id, replicationCount, expectedGroups);
        return result == null ? NotFound(new { message = "Khong the kiem tra tai nguyen." }) : Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewExperimentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Du lieu khong hop le." });

        var userId = GetUserId();
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { message = "Chi quan ly moi duoc duyet yeu cau." });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = "Khong tim thay yeu cau." });

        var farm = await _farmRepository.GetByIdAsync(existing.FarmId);
        if (farm == null) return NotFound(new { message = "Trai khong ton tai." });
        if (farm.ManagerId == null)
            return StatusCode(403, new { message = "Trai chua co quan ly." });
        if (farm.ManagerId != userId)
            return StatusCode(403, new { message = "Ban khong phai la quan ly trai nay." });

        if (!string.Equals(existing.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = $"Yeu cau da o trang thai '{existing.Status}'. Khong the duyet lai." });

        if (dto.Result == ReviewResult.Approved && (dto.ReservedBedIds == null || dto.ReservedBedIds.Count == 0))
            return BadRequest(new { message = "Khi duyet yeu cau, can chon it nhat mot lo de giu cho." });

        try
        {
            var result = await _service.ReviewAsync(id, dto, userId);
            return result == null
                ? StatusCode(500, new { message = "Duyet yeu cau that bai." })
                : Ok(new { success = true, message = "Duyet yeu cau thanh cong.", data = result });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
    }

    [HttpGet("{id:guid}/reserved-beds")]
    public async Task<IActionResult> GetReservedBeds(Guid id)
    {
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { message = "Chi quan ly moi duoc xem." });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = "Khong tim thay yeu cau." });

        var farm = await _farmRepository.GetByIdAsync(existing.FarmId);
        if (farm == null || farm.ManagerId != GetUserId())
            return StatusCode(403, new { message = "Ban khong phai la quan ly trai nay." });

        var result = await _service.GetReservedBedsAsync(id);
        return result == null
            ? NotFound(new { message = "Khong tim thay phieu giu cho nao." })
            : Ok(result);
    }

    [HttpGet("manager/inbox")]
    public async Task<IActionResult> ManagerInbox([FromQuery] string? status)
    {
        var userId = GetUserId();
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { message = "Chi quan ly moi duoc truy cap hop thu den." });

        RequestStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<RequestStatus>(status, ignoreCase: true, out var parsed))
                return BadRequest(new { message = $"Trang thai '{status}' khong hop le. Cac gia tri hop le: Pending, Approved, Rejected, Cancelled." });
            statusFilter = parsed;
        }

        var requests = await _service.GetByManagerAsync(userId, statusFilter);
        return Ok(new { success = true, message = "Lay danh sach hop thu thanh cong.", data = requests });
    }

    public class StatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
