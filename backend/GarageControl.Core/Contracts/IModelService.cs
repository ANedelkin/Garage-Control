using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IModelService
    {
        Task<IEnumerable<ModelVM>> GetModels(string makeId);
        Task CreateModel(ModelVM model, string userId);
        Task UpdateModel(ModelVM model);
        Task DeleteModel(string id);
    }
}
