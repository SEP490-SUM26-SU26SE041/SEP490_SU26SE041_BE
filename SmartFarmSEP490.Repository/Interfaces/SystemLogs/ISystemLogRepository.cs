using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.SystemLogs
{
    public interface ISystemLogRepository
    {
        Task<SystemLog> AddLogAsync(SystemLog log);
        Task<(List<SystemLog> Items, int TotalCount)> GetLogsAsync(string action, string entityName, string searchTerm, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
    }
}
