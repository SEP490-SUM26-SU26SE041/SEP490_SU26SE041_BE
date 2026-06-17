using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.SystemLogs;

namespace SmartFarmSEP490.Repository.Implementations.SystemLogs
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly SmartFarmDbContext _context;

        public SystemLogRepository(SmartFarmDbContext context)
        {
            _context = context;
        }

        public async Task<SystemLog> AddLogAsync(SystemLog log)
        {
            await _context.SystemLogs.AddAsync(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<(List<SystemLog> Items, int TotalCount)> GetLogsAsync(string action, string entityName, string searchTerm, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            var query = _context.SystemLogs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(l => l.Action == action);
            }

            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(l => l.EntityName == entityName);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(l => (l.Description != null && l.Description.ToLower().Contains(lowerSearch)) || 
                                         (l.Action != null && l.Action.ToLower().Contains(lowerSearch)) ||
                                         (l.EntityName != null && l.EntityName.ToLower().Contains(lowerSearch)));
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= endDate.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
