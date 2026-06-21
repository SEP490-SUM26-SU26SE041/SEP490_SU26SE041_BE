using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ExperimentRequests;

public class RequestReviewRepository : IRequestReviewRepository
{
    private readonly SmartFarmDbContext _context;
    public RequestReviewRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.RequestReview> CreateAsync(M.RequestReview entity)
    {
        entity.ReviewedAt = DateTime.UtcNow;
        await _context.RequestReviews.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<M.RequestReview>> GetByRequestIdAsync(Guid requestId) =>
        await _context.RequestReviews
            .Include(rr => rr.Reviewer).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(rr => rr.RequestId == requestId)
            .OrderByDescending(rr => rr.ReviewedAt)
            .ToListAsync();
}
