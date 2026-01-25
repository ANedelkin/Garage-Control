using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IModelService
    {
        Task<IEnumerable<ModelVM>> GetModels(string makeId, string userId);
        Task<ModelVM?> GetModel(string id);
        Task CreateModel(ModelVM model, string userId);
        Task UpdateModel(ModelVM model, string userId);
        Task DeleteModel(string id, string userId);
    }
}
