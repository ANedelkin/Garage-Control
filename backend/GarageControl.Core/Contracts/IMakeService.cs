using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IMakeService
    {
        Task<IEnumerable<MakeVM>> GetMakes(string userId);
        Task<MakeVM?> GetMake(string id);
        Task CreateMake(MakeVM model, string userId);
        Task UpdateMake(MakeVM model);
        Task DeleteMake(string id);
    }
}
