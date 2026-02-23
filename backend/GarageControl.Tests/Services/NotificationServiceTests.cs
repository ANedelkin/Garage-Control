using Xunit;
using GarageControl.Core.Services;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GarageControl.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _service = new NotificationService(_context);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnNotificationsForUser()
        {
            // Arrange
            var userId = "u1";
            _context.Notifications.AddRange(new List<Notification>
            {
                new Notification { Id = "n1", UserId = userId, Message = "M1", CreatedAt = DateTime.UtcNow },
                new Notification { Id = "n2", UserId = "other", Message = "M2", CreatedAt = DateTime.UtcNow }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserNotificationsAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("M1", result[0].Message);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldUpdateIsRead()
        {
            // Arrange
            var userId = "u1";
            var notification = new Notification { Id = "n1", UserId = userId, IsRead = false, Message = "M1", CreatedAt = DateTime.UtcNow };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            await _service.MarkAsReadAsync("n1", userId);

            // Assert
            var inDb = await _context.Notifications.FindAsync("n1");
            Assert.NotNull(inDb);
            Assert.True(inDb.IsRead);
        }

        [Fact]
        public async Task SendStockNotificationAsync_ShouldNotifyWorkersAndOwner_AndUpdatesExisting()
        {
            // Arrange
            var workshopId = "w1";
            var ownerId = "owner";
            var workerId = "worker";
            
            var workshop = new Workshop { Id = workshopId, BossId = ownerId, Name = "W", Address = "123 Main St", PhoneNumber = "555-1234" };
            _context.Workshops.Add(workshop);
            
            var access = new Access { Id = "a1", Name = "Parts Stock" };
            _context.Accesses.Add(access);
            await _context.SaveChangesAsync();
            
            var worker = new Worker { Id = "wkr1", UserId = workerId, WorkshopId = workshopId, Name = "Wkr" };
            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();
            
            // Add the access to the worker
            worker.Accesses.Add(access);
            await _context.SaveChangesAsync();

            // Act 1: Initial creation
            await _service.SendStockNotificationAsync(workshopId, "p1", "Part A", 5, 10);

            // Assert 1
            var notifications = _context.Notifications.ToList();
            Assert.Equal(2, notifications.Count);
            Assert.Contains(notifications, n => n.UserId == ownerId && n.Message.Contains("5"));
            Assert.Contains(notifications, n => n.UserId == workerId && n.Message.Contains("5"));

            // Act 2: Update existing
            await _service.SendStockNotificationAsync(workshopId, "p1", "Part A", 4, 10);

            // Assert 2
            notifications = _context.Notifications.ToList();
            Assert.Equal(2, notifications.Count); // Count should still be 2
            Assert.Contains(notifications, n => n.UserId == ownerId && n.Message.Contains("4"));
            Assert.Contains(notifications, n => n.UserId == workerId && n.Message.Contains("4"));
        }

        [Fact]
        public async Task RemoveStockNotificationAsync_ShouldDeleteMatchingNotifications()
        {
            // Arrange
            _context.Notifications.AddRange(new List<Notification>
            {
                new Notification { Id = "n1", UserId = "u1", Message = "Part 'A' is low on stock (Available: 5, Minimum: 10).", Link = "/parts?partId=p1", CreatedAt = DateTime.UtcNow },
                new Notification { Id = "n2", UserId = "u2", Message = "Part 'A' is low on stock (Available: 5, Minimum: 10).", Link = "/parts?partId=p1", CreatedAt = DateTime.UtcNow },
                new Notification { Id = "n3", UserId = "u1", Message = "Other msg", Link = "/parts?partId=p2", CreatedAt = DateTime.UtcNow }
            });
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoveStockNotificationAsync("w1", "p1");

            // Assert
            var remaining = _context.Notifications.ToList();
            Assert.Single(remaining);
            Assert.Equal("n3", remaining[0].Id);
        }
    }
}
