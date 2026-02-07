using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Core.Contracts
{
    public interface IFolderService
    {
        Task<FolderContentViewModel> GetFolderContentAsync(string garageId, string? folderId);
        Task<PartsFolderViewModel> CreateFolderAsync(string userId, string garageId, CreateFolderViewModel model);
        Task RenameFolderAsync(string userId, string garageId, string folderId, string newName);
        Task DeleteFolderAsync(string userId, string garageId, string folderId);
        Task MoveFolderAsync(string userId, string garageId, string folderId, string? newParentId);
    }
}