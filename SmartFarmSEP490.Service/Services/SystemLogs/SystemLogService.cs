using System;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.Interfaces.SystemLogs;
using SmartFarmSEP490.Service.Interfaces.SystemLogs;

namespace SmartFarmSEP490.Service.Services.SystemLogs
{
    public class SystemLogService : ISystemLogService
    {
        private readonly ISystemLogRepository _repository;

        public SystemLogService(ISystemLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedList<SystemLogDto>> GetLogsAsync(SystemLogFilterDto filter)
        {
            var result = await _repository.GetLogsAsync(
                filter.Action,
                filter.EntityName,
                filter.SearchTerm,
                filter.StartDate,
                filter.EndDate,
                filter.PageNumber,
                filter.PageSize
            );

            var dtos = result.Items.Select(l => new SystemLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserEmail = l.User?.Email,
                Action = l.Action,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Description = l.Description,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                Metadata = l.Metadata?.RootElement.ToString(),
                CreatedAt = l.CreatedAt
            }).ToList();

            return new PaginatedList<SystemLogDto>(dtos, result.TotalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<SystemLogDto> AddMockLogAsync()
        {
            var random = new Random();
            var scenarios = new[]
            {
                new { Action = "LOGIN", Entity = "Auth", Desc = "Người dùng đăng nhập vào hệ thống thành công." },
                new { Action = "CREATE", Entity = "User", Desc = "Tạo mới tài khoản người dùng: nguyenvana@smartfarm.com." },
                new { Action = "UPDATE", Entity = "Farm", Desc = "Chỉnh sửa thông tin Nông trại: Farm A1." },
                new { Action = "DELETE", Entity = "Device", Desc = "Xóa thiết bị cảm biến SN-TH-022 khỏi hệ thống." },
                new { Action = "APPROVE", Entity = "ExperimentRequest", Desc = "Duyệt yêu cầu thí nghiệm: Đánh giá phân bón mới." },
                new { Action = "REASSIGN", Entity = "Task", Desc = "Phân công lại công việc: Tưới nước khu A cho Kỹ thuật viên B." },
                new { Action = "EXPORT", Entity = "ExperimentReport", Desc = "Xuất báo cáo tổng kết thí nghiệm ra định dạng PDF." },
                new { Action = "ERROR", Entity = "SystemAPI", Desc = "Lỗi kết nối đến thiết bị IoT Gateway (Timeout 5000ms)." }
            };

            var scenario = scenarios[random.Next(scenarios.Length)];

            var log = new SystemLog
            {
                Id = Guid.NewGuid(),
                Action = scenario.Action,
                EntityName = scenario.Entity,
                EntityId = Guid.NewGuid(),
                Description = scenario.Desc,
                IpAddress = "192.168.1." + random.Next(1, 255),
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                Metadata = System.Text.Json.JsonDocument.Parse("{\"mock\": true}"),
                CreatedAt = DateTime.UtcNow,
                UserId = scenario.Action != "ERROR" ? Guid.NewGuid() : null
            };

            await _repository.AddLogAsync(log);

            return new SystemLogDto
            {
                Id = log.Id,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Description = log.Description,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Metadata = log.Metadata?.RootElement.ToString(),
                CreatedAt = log.CreatedAt
            };
        }
    }
}
