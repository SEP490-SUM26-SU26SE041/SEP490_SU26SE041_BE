using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;

public interface IRequestReviewRepository
{
    Task<M.RequestReview> CreateAsync(M.RequestReview entity);
    Task<List<M.RequestReview>> GetByRequestIdAsync(Guid requestId);
}
