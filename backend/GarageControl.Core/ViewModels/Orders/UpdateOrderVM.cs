using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Orders
{
    public class UpdateOrderVM
    {
        [Required]
        public string CarId { get; set; } = null!;
        public int Kilometers { get; set; }
        public bool IsArchived { get; set; }
    }
}
