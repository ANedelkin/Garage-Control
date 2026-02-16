using System.ComponentModel.DataAnnotations;
using GarageControl.Core.ViewModels.Jobs;

namespace GarageControl.Core.ViewModels.Orders
{
    public class UpdateOrderVM
    {
        [Required]
        public string CarId { get; set; } = null!;
        public List<UpdateJobVM> Jobs { get; set; } = new List<UpdateJobVM>();
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
    }
}
