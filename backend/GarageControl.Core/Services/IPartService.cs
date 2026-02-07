using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Core.Services
{
    public interface IPartService
    {
        Task<FolderContentViewModel> GetFolderContentAsync(string garageId, string? folderId);
        Task<List<PartViewModel>> GetAllPartsAsync(string garageId);
        Task<PartViewModel> CreatePartAsync(string userId, string garageId, CreatePartViewModel model);
        Task<PartWithPathViewModel?> GetPartAsync(string garageId, string partId);
        Task EditPartAsync(string userId, string garageId, UpdatePartViewModel model);
        Task DeletePartAsync(string userId, string garageId, string partId);
        Task<PartsFolderViewModel> CreateFolderAsync(string userId, string garageId, CreateFolderViewModel model);
        Task RenameFolderAsync(string userId, string garageId, string folderId, string newName);
        Task DeleteFolderAsync(string userId, string garageId, string folderId);
        Task MovePartAsync(string userId, string garageId, string partId, string? newParentId);
        Task MoveFolderAsync(string userId, string garageId, string folderId, string? newParentId);
        Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null);
    }
}
