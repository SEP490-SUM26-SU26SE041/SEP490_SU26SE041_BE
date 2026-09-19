using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Interfaces.AI;

/// <summary>
/// Service xử lý AI analysis: enqueue, gọi AI, parse response, persist.
/// </summary>
public interface IAIAnalysisService
{
    /// <summary>
    /// Enqueue một PlantImage để Worker xử lý AI.
    /// Idempotent: nếu PlantImage đã có AIAnalysis Completed thì bỏ qua.
    /// </summary>
    Task EnqueueAsync(Guid plantImageId, CancellationToken ct = default);

    /// <summary>
    /// Worker entry: xử lý 1 PlantImage — gọi AI, parse, lưu DB.
    /// Trả về AIAnalysis (Failed status nếu lỗi).
    /// </summary>
    Task<AIAnalysis> ProcessAsync(Guid plantImageId, CancellationToken ct = default);

    /// <summary>
    /// FE poll: lấy list PlantImage + AI status của một TaskReport.
    /// </summary>
    Task<List<PlantImageWithAnalysisDto>> GetByTaskReportAsync(Guid taskReportId, bool includeAnalysis, CancellationToken ct = default);
}
