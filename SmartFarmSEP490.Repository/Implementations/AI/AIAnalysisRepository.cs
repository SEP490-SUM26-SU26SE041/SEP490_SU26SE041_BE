using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Repository.Implementations.AI;

public class AIAnalysisRepository : IAIAnalysisRepository
{
    private readonly SmartFarmDbContext _context;

    public AIAnalysisRepository(SmartFarmDbContext context) => _context = context;

    public async Task<AIAnalysis?> GetByIdAsync(Guid id) =>
        await _context.AIAnalyses.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<AIAnalysis?> GetByPlantImageIdAsync(Guid plantImageId) =>
        await _context.AIAnalyses.FirstOrDefaultAsync(x => x.PlantImageId == plantImageId);

    public async Task<List<AIAnalysis>> GetByTaskReportIdAsync(Guid taskReportId) =>
        await _context.AIAnalyses
            .Where(x => x.TaskReportId == taskReportId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<AIAnalysis> AddAsync(AIAnalysis entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
        if (entity.UpdatedAt == default) entity.UpdatedAt = DateTime.UtcNow;
        await _context.AIAnalyses.AddAsync(entity);
        return entity;
    }

    public void Update(AIAnalysis entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.AIAnalyses.Update(entity);
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
