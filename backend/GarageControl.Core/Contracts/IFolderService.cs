using GarageControl.Core.ViewModels;

namespace GarageControl.Core.Contracts
{
    public interface IFolderService
    {
        Task<FolderContentVM> GetFolderContentAsync(string garageId, string? folderId);
        Task<PartsFolderVM> CreateFolderAsync(string userId, string garageId, CreateFolderVM model);
        Task RenameFolderAsync(string userId, string garageId, string folderId, string newName);
        Task DeleteFolderAsync(string userId, string garageId, string folderId);
        Task MoveFolderAsync(string userId, string garageId, string folderId, string? newParentId);
    }
}