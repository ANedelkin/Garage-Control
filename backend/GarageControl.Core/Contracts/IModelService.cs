using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;

namespace GarageControl.Core.Contracts
{
    public interface IModelService
    {
        Task<IEnumerable<ModelVM>> GetModels(string makeId, string userId);
        Task<ModelVM?> GetModel(string id);
        Task<ModelVM?> GetModel(string id, string userId);
        Task CreateModel(ModelVM model, string userId);
        Task UpdateModel(string id, ModelVM model, string userId);
        Task DeleteModel(string id, string userId);
        Task MergeModelWithGlobal(string customModelId, string globalModelId, string userId);
    }
}
