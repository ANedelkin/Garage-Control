using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GarageControl.Core.ViewModels.Jobs;

namespace GarageControl.Core.ViewModels.Orders
{
    public class CreateOrderVM
    {
        [Required]
        public string CarId { get; set; } = null!;
        public List<CreateJobVM> Jobs { get; set; } = new List<CreateJobVM>();
        public int Kilometers { get; set; }
    }
}
