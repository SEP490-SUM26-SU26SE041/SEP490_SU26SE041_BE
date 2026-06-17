using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.SystemLogs;

namespace SmartFarmSEP490.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemLogsController : ControllerBase
    {
        private readonly ISystemLogService _systemLogService;

        public SystemLogsController(ISystemLogService systemLogService)
        {
            _systemLogService = systemLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] SystemLogFilterDto filter)
        {
            try
            {
                var logs = await _systemLogService.GetLogsAsync(filter);
                return Ok(logs);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("mock")]
        public async Task<IActionResult> AddMockLog()
        {
            var log = await _systemLogService.AddMockLogAsync();
            return Ok(log);
        }
    }
}
