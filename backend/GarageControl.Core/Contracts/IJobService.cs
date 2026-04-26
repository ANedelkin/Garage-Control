using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Shared;

public interface IJobService
{
    Task<MethodResponseVM> CreateJobAsync(
        string userId,
        string orderId,
        string workshopId,
        CreateJobVM model);

    Task<MethodResponseVM> UpdateJobAsync(
        string userId,
        string jobId,
        string workshopId,
        UpdateJobVM model);

    Task<JobDetailsVM?> GetJobByIdAsync(
        string jobId,
        string workshopId);

    Task<JobDetailsVM?> GetCompletedJobByIdAsync(
        string jobId,
        string workshopId);

    Task<List<JobToDoVM>> GetMyJobsAsync(
        string userId,
        string workshopId);

    Task<List<JobToDoVM>> GetJobsByWorkerIdAsync(
        string workerId,
        string workshopId);

    Task<List<JobListVM>> GetJobsByOrderIdAsync(
        string orderId,
        string workshopId);

    Task<MethodResponseVM> DeleteJobAsync(
        string userId,
        string jobId,
        string workshopId,
        bool skipLogging = false);

    Task<List<BusySlotVM>> GetBusySlotsAsync(
        string workerId,
        DateTime start,
        DateTime end,
        string? excludeJobId = null);
}
