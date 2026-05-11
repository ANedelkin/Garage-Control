using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Jobs
{
    public class ValidJobPartAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (CreateJobPartVM)validationContext.ObjectInstance;
            if (string.IsNullOrWhiteSpace(model.PartId))
            {
                if (string.IsNullOrWhiteSpace(model.PartName))
                    return new ValidationResult("Part name required", new[] { nameof(model.PartId) });
                return new ValidationResult("Part doesn't exist", new[] { nameof(model.PartId) });
            }
            return ValidationResult.Success;
        }
    }

    public class CreateJobPartVM : IValidatableObject
    {
        [ValidJobPart]
        public string? PartId { get; set; }
        public string? PartName { get; set; }
        [Required(ErrorMessage = "Planned quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Planned quantity cannot be negative.")]
        public int? PlannedQuantity { get; set; }
        [Required(ErrorMessage = "Sent quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Sent quantity cannot be negative.")]
        public int? SentQuantity { get; set; }
        [Required(ErrorMessage = "Used quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Used quantity cannot be negative.")]
        public int? UsedQuantity { get; set; }
        [Required(ErrorMessage = "Requested quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Requested quantity cannot be negative.")]
        public int? RequestedQuantity { get; set; }

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
