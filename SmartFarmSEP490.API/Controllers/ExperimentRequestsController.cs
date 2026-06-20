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
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = GetRole();
        if (role != "Researcher")
            return StatusCode(403, new { error = "Only Researcher can create experiment requests" });

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
            return BadRequest(new { error = $"Farm {dto.FarmId} not found" });
        if (farm.ManagerId == null)
            return BadRequest(new { error = "Farm has no Manager yet; cannot submit request" });

        var result = await _service.CreateAsync(dto, userId);
        if (result == null)
            return StatusCode(500, new { error = "Create experiment request returned null" });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
        return StatusCode(403, new { error = "You do not have permission to view this request" });
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
            return StatusCode(403, new { error = "Only Researcher and Manager can list requests" });

        if (role == "Researcher")
        {
            // Luôn filter theo chính researcher đó
            var mine = await _service.GetByResearcherAsync(userId);
            if (!string.IsNullOrEmpty(status))
                return Ok(mine.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)));
            return Ok(mine);
        }

        // Manager: chỉ thấy request của các farm mình quản lý
        var myFarms = await _farmRepository.GetByManagerAsync(userId);
        var myFarmIds = myFarms.Select(f => f.Id).ToHashSet();
        if (myFarmIds.Count == 0) return Ok(new List<ExperimentRequestResponseDto>());

        var fromQuery = await _service.GetByFarmAsync(farmId ?? Guid.Empty);
        // nếu không truyền farmId, lấy theo từng farm của mình
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

    // ============ Researcher: update own request (only when Pending) ============

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExperimentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var role = GetRole();
        if (role != "Researcher") return StatusCode(403, new { error = "Only Researcher can update" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (existing.ResearcherId != userId) return StatusCode(403, new { error = "Not your request" });
        if (!string.Equals(existing.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only Pending requests can be updated" });

        var result = await _service.UpdateAsync(id, dto, userId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var role = GetRole();
        if (role != "Researcher") return StatusCode(403, new { error = "Only Researcher can delete" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (existing.ResearcherId != userId) return StatusCode(403, new { error = "Not your request" });

        await _service.DeleteAsync(id);
        return NoContent();
    }

    // ============ Manager: review (accept / reject) ============

    [HttpGet("{id:guid}/resource-summary")]
    public async Task<IActionResult> GetResourceSummary(Guid id)
    {
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { error = "Only Manager can view resource summary" });

        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        var farm = await _farmRepository.GetByIdAsync(request.FarmId);
        if (farm == null) return NotFound(new { error = "Farm not found" });
        if (farm.ManagerId != GetUserId())
            return StatusCode(403, new { error = "You are not the Manager of this farm" });

        var result = await _service.ValidateResourcesAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewExperimentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { error = "Only Manager can review experiment requests" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var farm = await _farmRepository.GetByIdAsync(existing.FarmId);
        if (farm == null) return NotFound(new { error = "Farm not found" });
        if (farm.ManagerId == null)
            return StatusCode(403, new { error = "Farm has no Manager assigned" });
        if (farm.ManagerId != userId)
            return StatusCode(403, new { error = "You are not the Manager of this farm" });

        if (!string.Equals(existing.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = $"Request is already {existing.Status}; cannot review again" });

        var result = await _service.ReviewAsync(id, dto, userId);
        return result == null ? StatusCode(500, new { error = "Review failed" }) : Ok(result);
    }

    // ============ Manager: inbox of requests from researchers ============

    [HttpGet("manager/inbox")]
    public async Task<IActionResult> ManagerInbox([FromQuery] string? status)
    {
        var userId = GetUserId();
        var role = GetRole();
        if (role != "Manager")
            return StatusCode(403, new { error = "Only Manager can access this inbox" });

        RequestStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<RequestStatus>(status, ignoreCase: true, out var parsed))
                return BadRequest(new
                {
                    error = $"Invalid status '{status}'. Allowed: Pending, Approved, Rejected, Cancelled (or omit to return all)"
                });
            statusFilter = parsed;
        }

        var requests = await _service.GetByManagerAsync(userId, statusFilter);
        return Ok(requests);
    }

    public class StatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
