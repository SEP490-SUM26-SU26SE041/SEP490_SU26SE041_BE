using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class MeasurementRecordRepository : IMeasurementRecordRepository
{
    private readonly SmartFarmDbContext _context;
    public MeasurementRecordRepository(SmartFarmDbContext context) => _context = context;

    private IQueryable<MeasurementRecord> BaseQuery(bool includeDeleted = false)
    {
        var query = _context.MeasurementRecords
            .Include(m => m.MeasuredByNavigation)
            .Include(m => m.MeasurementDefinition)
            .AsQueryable();
        if (!includeDeleted)
            query = query.Where(m => m.DeletedAt == null);
        return query;
    }

    public async Task<MeasurementRecord?> GetByIdAsync(Guid id, bool includeDeleted = false) =>
        await BaseQuery(includeDeleted).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<MeasurementRecord>> GetByBatchIdAsync(Guid batchId, bool includeDeleted = false) =>
        await BaseQuery(includeDeleted)
            .Where(m => m.BatchId == batchId)
            .OrderByDescending(m => m.MeasuredAt)
            .ToListAsync();

    public async Task<List<MeasurementRecord>> GetByExperimentIdAsync(Guid experimentId, bool includeDeleted = false) =>
        await BaseQuery(includeDeleted)
            .Where(m => m.ExperimentId == experimentId)
            .OrderByDescending(m => m.MeasuredAt)
            .ToListAsync();

    public async Task<List<MeasurementRecord>> GetByStageIdAsync(Guid stageId, bool includeDeleted = false) =>
        await BaseQuery(includeDeleted)
            .Where(m => m.ExperimentStageId == stageId)
            .OrderByDescending(m => m.MeasuredAt)
            .ToListAsync();

    public async Task<MeasurementRecord> CreateAsync(MeasurementRecord entity)
    {
        entity.MeasuredAt = DateTime.UtcNow;
        await _context.MeasurementRecords.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(MeasurementRecord entity)
    {
        _context.MeasurementRecords.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var record = await _context.MeasurementRecords.FindAsync(id);
        if (record != null)
        {
            record.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
