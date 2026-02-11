using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels
{
    public class CreateOrderVM
    {
        [Required]
        public string CarId { get; set; } = null!;
        public List<CreateJobVM> Jobs { get; set; } = new List<CreateJobVM>();
        public int Kilometers { get; set; }
    }
}
