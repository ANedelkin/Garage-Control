using GarageControl.Core.Models;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;

public interface IJobService
{
    Task<MethodResponse> CreateJobAsync(
        string userId,
        string orderId,
        string workshopId,
        CreateJobViewModel model);

    Task<MethodResponse> UpdateJobAsync(
        string userId,
        string jobId,
        string workshopId,
        UpdateJobViewModel model);

    Task<JobDetailsViewModel?> GetJobByIdAsync(
        string jobId,
        string workshopId);

    Task<List<JobToDoViewModel>> GetMyJobsAsync(
        string userId,
        string workshopId);
}
