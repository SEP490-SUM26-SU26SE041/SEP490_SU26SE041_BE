using SmartFarmSEP490.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.AI;

public interface IAIAnalysisRepository
{
    Task<AIAnalysis?> GetByIdAsync(Guid id);

    Task<AIAnalysis?> GetByPlantImageIdAsync(Guid plantImageId);

    Task<List<AIAnalysis>> GetByTaskReportIdAsync(Guid taskReportId);

    Task<AIAnalysis> AddAsync(AIAnalysis entity);

    void Update(AIAnalysis entity);

    /// <summary>Persist pending changes. Trả về số row affected.</summary>
    Task<int> SaveChangesAsync();
}
