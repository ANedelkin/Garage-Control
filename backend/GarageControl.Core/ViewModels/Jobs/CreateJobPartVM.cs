using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Jobs
{
    public class CreateJobPartVM : IValidatableObject
    {
        [Required(ErrorMessage = "Part doesn't exist")]
        public string PartId { get; set; } = null!;
        [Range(0, int.MaxValue)]
        public int PlannedQuantity { get; set; }
        [Range(0, int.MaxValue)]
        public int SentQuantity { get; set; }
        [Range(0, int.MaxValue)]
        public int UsedQuantity { get; set; }
        [Range(0, int.MaxValue)]
        public int RequestedQuantity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SentQuantity > PlannedQuantity)
            {
                yield return new ValidationResult(
                    "Sent quantity cannot be greater than planned quantity.",
                    new[] { nameof(SentQuantity) });
            }

            if (UsedQuantity > SentQuantity)
            {
                yield return new ValidationResult(
                    "Used quantity cannot be greater than sent quantity.",
                    new[] { nameof(UsedQuantity) });
            }
        }
    }
}
