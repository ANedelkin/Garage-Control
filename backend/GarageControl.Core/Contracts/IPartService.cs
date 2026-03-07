using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Core.Contracts
{
    public interface IPartService
    {
        Task<List<PartVM>> GetAllPartsAsync(string workshopId);
        Task<PartVM?> GetPartByIdAsync(string partId, string workshopId);
        Task<List<PartVM>> GetPartsByFolderAsync(string? folderId, string workshopId);
        Task<PartVM> CreatePartAsync(string userId, string workshopId, CreatePartVM model);
        Task<PartVM> EditPartAsync(string userId, string workshopId, string partId, UpdatePartVM model);
        Task<PartWithPathVM?> GetPartWithPathAsync(string partId, string workshopId);
        Task MovePartAsync(string userId, string garageId, string partId, string? newParentId);
        Task DeletePartAsync(string userId, string workshopId, string partId);
    }
}
