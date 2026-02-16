using Xunit;
using System.Threading.Tasks;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using System.Collections.Generic;

namespace GarageControl.Tests.Services
{
    public class PDFGeneratorServiceTests
    {
        private readonly PDFGeneratorService _service;

        public PDFGeneratorServiceTests()
        {
            _service = new PDFGeneratorService();
        }

        [Fact]
        public async Task GenerateInvoicePdfAsync_ShouldReturnByteArray()
        {
            // Arrange
            var order = new OrderInvoiceVM
            {
                OrderId = "o1",
                ClientName = "C",
                CarName = "Car",
                CarRegistrationNumber = "R",
                WorkshopName = "W",
                Jobs = new List<JobInvoiceVM>
                {
                    new JobInvoiceVM
                    {
                        JobTypeName = "J",
                        LaborCost = 100,
                        Parts = new List<JobPartDetailsVM>
                        {
                            new JobPartDetailsVM { PartName = "P", Price = 10, UsedQuantity = 1 }
                        }
                    }
                }
            };

            // Act
            var result = await _service.GenerateInvoicePdfAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
