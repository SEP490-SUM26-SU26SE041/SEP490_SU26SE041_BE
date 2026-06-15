using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ProcedureTemplates;

public interface IProcedureTemplateRepository
{
    Task<M.ProcedureTemplate?> GetByIdAsync(Guid id);
    Task<M.ProcedureTemplate?> GetByIdWithStepsAsync(Guid id);
    Task<List<M.ProcedureTemplate>> GetAllAsync();
    Task<List<M.ProcedureTemplate>> GetByCropVarietyAsync(Guid cropVarietyId);
    Task<M.ProcedureTemplate> CreateAsync(M.ProcedureTemplate entity);
    Task UpdateAsync(M.ProcedureTemplate entity);
    Task DeleteAsync(Guid id);
}
