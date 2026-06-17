using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.SystemLogs
{
    public interface ISystemLogService
    {
        Task<PaginatedList<SystemLogDto>> GetLogsAsync(SystemLogFilterDto filter);
        Task<SystemLogDto> AddMockLogAsync();
    }
}
