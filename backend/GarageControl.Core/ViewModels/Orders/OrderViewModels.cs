using GarageControl.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Orders
{
    public class OrderListViewModel
    {
        public string Id { get; set; } = null!;
        public string CarId { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
        public List<JobListViewModel> Jobs { get; set; } = new List<JobListViewModel>();
    }

    public class JobListViewModel
    {
        public string Id { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string MechanicName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LaborCost { get; set; }
    }

    public class CreateOrderViewModel
    {
        [Required]
        public string CarId { get; set; } = null!;
        public List<CreateJobViewModel> Jobs { get; set; } = new List<CreateJobViewModel>();
        public int Kilometers { get; set; }
    }

    public class CreateJobViewModel
    {
        [Required]
        public string JobTypeId { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string WorkerId { get; set; } = null!;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public decimal LaborCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<CreateJobPartViewModel> Parts { get; set; } = new List<CreateJobPartViewModel>();
    }

    public class CreateJobPartViewModel
    {
        [Required]
        public string PartId { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class UpdateOrderViewModel
    {
        [Required]
        public string CarId { get; set; } = null!;
        public List<UpdateJobViewModel> Jobs { get; set; } = new List<UpdateJobViewModel>();
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
    }

    public class UpdateJobViewModel
    {
        public string? Id { get; set; } // If null, it's a new job
        [Required]
        public string JobTypeId { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string WorkerId { get; set; } = null!;
        public JobStatus Status { get; set; }
        public decimal LaborCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<CreateJobPartViewModel> Parts { get; set; } = new List<CreateJobPartViewModel>();
    }

    public class OrderDetailsViewModel
    {
        public string Id { get; set; } = null!;
        public string CarId { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
        public List<JobDetailsViewModel> Jobs { get; set; } = new List<JobDetailsViewModel>();
    }

    public class JobDetailsViewModel
    {
        public string Id { get; set; } = null!;
        public string JobTypeId { get; set; } = null!;
        public string WorkerId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public JobStatus Status { get; set; }
        public decimal LaborCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<JobPartDetailsViewModel> Parts { get; set; } = new List<JobPartDetailsViewModel>();
    }

    public class JobPartDetailsViewModel
    {
        public string PartId { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
