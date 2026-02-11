using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels
{
    public class JobTypeVM
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(JobTypeConstants.nameMaxLength, MinimumLength = JobTypeConstants.nameMinLength)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public List<string> Mechanics { get; set; } = new();
    }
}
