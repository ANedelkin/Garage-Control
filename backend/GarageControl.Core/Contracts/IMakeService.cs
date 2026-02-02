using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IMakeService
    {
        Task<IEnumerable<MakeVM>> GetMakes(string userId);
        Task<MakeVM?> GetMake(string id);
        Task<string> CreateMake(MakeVM model, string userId);
        Task UpdateMake(MakeVM model, string userId);
        Task DeleteMake(string id, string userId);
        Task<IEnumerable<MetricSuggestionVM>> GetSuggestions();
        Task<IEnumerable<MetricSuggestionVM>> GetSuggestedModels(string makeName);
        Task PromoteSuggestion(string name, string? newName);
        Task PromoteModelSuggestion(string makeName, string modelName, string? newModelName, string? newMakeName);
        Task MergeMakeWithGlobal(string customMakeId, string globalMakeId, string userId);
    }
}
