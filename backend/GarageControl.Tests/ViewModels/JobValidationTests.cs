using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GarageControl.Core.ViewModels.Jobs;
using Xunit;

namespace GarageControl.Tests.ViewModels
{
    public class JobValidationTests
    {
        [Fact]
        public void CreateJobVM_MissingFields_ShouldReturnCustomErrorMessages()
        {
            // Arrange
            var model = new CreateJobVM
            {
                JobTypeId = null!,
                WorkerId = null!,
                StartTime = null,
                EndTime = null
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.ErrorMessage == "Job type required" && v.MemberNames.Contains("JobTypeId"));
            Assert.Contains(validationResults, v => v.ErrorMessage == "Mechanic required" && v.MemberNames.Contains("WorkerId"));
            Assert.Contains(validationResults, v => v.ErrorMessage == "Time slot required" && v.MemberNames.Contains("StartTime"));
        }

        [Fact]
        public void UpdateJobVM_MissingFields_ShouldReturnCustomErrorMessages()
        {
            // Arrange
            var model = new UpdateJobVM
            {
                JobTypeId = null!,
                WorkerId = null!,
                StartTime = null,
                EndTime = null
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.ErrorMessage == "Job type required" && v.MemberNames.Contains("JobTypeId"));
            Assert.Contains(validationResults, v => v.ErrorMessage == "Mechanic required" && v.MemberNames.Contains("WorkerId"));
            Assert.Contains(validationResults, v => v.ErrorMessage == "Time slot required" && v.MemberNames.Contains("StartTime"));
        }

        [Fact]
        public void CreateJobPartVM_MissingPartId_ShouldReturnCustomErrorMessage()
        {
            // Arrange
            var model = new CreateJobPartVM
            {
                PartId = null!
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.ErrorMessage == "Part doesn't exist" && v.MemberNames.Contains("PartId"));
        }

        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
