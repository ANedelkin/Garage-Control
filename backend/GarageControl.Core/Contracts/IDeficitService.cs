using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.Contracts
{
    public interface IDeficitService
    {
        /// <summary>
        /// Calculates the deficit status for a part based on its availability balance and minimum quantity.
        /// </summary>
        DeficitStatus CalculatePartDeficitStatus(Part part);

        /// <summary>
        /// Updates the deficit status of a part and propagates the change to all ancestors.
        /// </summary>
        Task UpdatePartDeficitStatusAsync(string workshopId, Part part);

        /// <summary>
        /// Recalculates deficit status for all parts in the workshop.
        /// </summary>
        Task RecalculateAllDeficitStatusesAsync(string workshopId);

        /// <summary>
        /// Recalculates deficit counts for a specific folder and propagates upward.
        /// </summary>
        Task RecalculateFolderDeficitCountsAsync(string folderId);
    }
}
