using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Orders
{
    public class CreateOrderVM
    {
        [Required]
        public string CarId { get; set; } = null!;
        public int Kilometers { get; set; }
    }
}
