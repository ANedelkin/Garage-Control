using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.ViewModels;
using System;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Tests.Services
{
    public class ModelServiceTests
    {
        private readonly Mock<IRepository> _mockRepo;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly ModelService _service;

        public ModelServiceTests()
        {
            _mockRepo = new Mock<IRepository>();
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new ModelService(_mockRepo.Object, _mockWorkshopService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task CreateModel_ShouldAddModelAndLog()
        {
            // Arrange
            var userId = "u1";
            var bossId = "b1";
            var workshopId = "w1";
            var model = new ModelVM { Name = "Corolla", MakeId = "m1" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };

            _mockWorkshopService.Setup(x => x.GetWorkshopBossId(userId)).ReturnsAsync(bossId);
            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);
            _mockRepo.Setup(x => x.GetByIdAsync<CarMake>("m1")).ReturnsAsync(make);

            // Act
            await _service.CreateModel(model, userId);

            // Assert
            _mockRepo.Verify(x => x.AddAsync(It.Is<CarModel>(m => m.Name == "Corolla" && m.CarMakeId == "m1" && m.CreatorId == bossId)), Times.Once);
            _mockRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, It.Is<string>(s => s.Contains("added model") && s.Contains("Corolla") && s.Contains("Toyota"))), Times.Once);
        }
    }
}
