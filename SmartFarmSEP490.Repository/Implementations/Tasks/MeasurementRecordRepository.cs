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

    public async Task<MeasurementRecord?> GetByIdAsync(Guid id) =>
        await _context.MeasurementRecords
            .Include(m => m.MeasuredByNavigation)
            .Include(m => m.MeasurementDefinition)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<MeasurementRecord>> GetByBatchIdAsync(Guid batchId) =>
        await _context.MeasurementRecords
            .Include(m => m.MeasuredByNavigation)
            .Include(m => m.MeasurementDefinition)
            .Where(m => m.BatchId == batchId)
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

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.MeasurementRecords.FindAsync(id);
        if (e != null) { _context.MeasurementRecords.Remove(e); await _context.SaveChangesAsync(); }
    }
}
