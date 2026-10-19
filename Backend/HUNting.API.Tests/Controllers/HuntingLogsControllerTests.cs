using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using HUNting.API.Controllers;
using HUNting.API.Data;
using HUNting.API.Models.Hunting;
using HUNting.API.Models.Users;

namespace HUNting.API.Tests.Controllers
{
    public class HuntingLogsControllerTests : IDisposable
    {
        private readonly HuntingContext _context;
        private readonly HuntingLogsController _controller;
        private readonly int _ownLogId;   // 1-es user naploja
        private readonly int _otherLogId; // 2-es user naploja

        public HuntingLogsControllerTests()
        {
            var options = new DbContextOptionsBuilder<HuntingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new HuntingContext(options);

            _context.Users.AddRange(
                new ApplicationUser { Id = 1, UserName = "hunter1", Email = "h1@test.com", FullName = "Hunter Egy" },
                new ApplicationUser { Id = 2, UserName = "hunter2", Email = "h2@test.com", FullName = "Hunter Ketto" });

            var ownLog = new HuntingLog { UserId = 1, OccurredAt = DateTime.UtcNow.AddDays(-1), AnimalType = "Oz", ShotFired = true };
            var otherLog = new HuntingLog { UserId = 2, OccurredAt = DateTime.UtcNow.AddDays(-2), AnimalType = "Vaddiszno", ShotFired = false };
            _context.HuntingLogs.AddRange(ownLog, otherLog);
            _context.SaveChanges();

            _ownLogId = ownLog.HuntingLogId;
            _otherLogId = otherLog.HuntingLogId;

            var envMock = new Mock<IWebHostEnvironment>();
            _controller = new HuntingLogsController(_context, envMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetUserContext(int userId, string role = "Hunter", bool societyAccount = false, string? queryString = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            if (societyAccount)
            {
                claims.Add(new Claim("account_type", "Society"));
            }

            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock")) };
            if (queryString != null)
            {
                httpContext.Request.QueryString = new QueryString(queryString);
            }

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyOwnLogs()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var logs = Assert.IsAssignableFrom<IEnumerable<HuntingLog>>(okResult.Value);
            var log = Assert.Single(logs);
            Assert.Equal(1, log.UserId);
        }

        [Fact]
        public async Task GetAll_AdminWithScopeAll_ReturnsAllLogs()
        {
            // Arrange
            SetUserContext(99, role: "Administrator", queryString: "?scope=all");

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var logs = Assert.IsAssignableFrom<IEnumerable<HuntingLog>>(okResult.Value);
            Assert.Equal(2, logs.Count());
        }

        [Fact]
        public async Task GetAll_AdminWithoutScope_ReturnsOnlyOwnLogs()
        {
            // Arrange
            SetUserContext(99, role: "Administrator");

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var logs = Assert.IsAssignableFrom<IEnumerable<HuntingLog>>(okResult.Value);
            Assert.Empty(logs);
        }

        [Fact]
        public async Task Get_OwnLog_ReturnsOk()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Get(_ownLogId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var log = Assert.IsType<HuntingLog>(okResult.Value);
            Assert.Equal(_ownLogId, log.HuntingLogId);
        }

        [Fact]
        public async Task Get_NonExistingLog_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Get(9999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_OthersLog_ReturnsForbid()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Get(_otherLogId);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task Get_OthersLog_AsAdmin_ReturnsOk()
        {
            // Arrange
            SetUserContext(99, role: "Administrator");

            // Act
            var result = await _controller.Get(_otherLogId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAndPersists()
        {
            // Arrange
            SetUserContext(1);
            var request = new HuntingLogsController.HuntingLogCreateRequest
            {
                OccurredAt = DateTime.UtcNow,
                LocationText = "Bakony",
                AnimalType = "Szarvas",
                ShotFired = true,
                Notes = "Hajnali les"
            };

            // Act
            var result = await _controller.Create(request);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("Get", created.ActionName);
            var log = Assert.IsType<HuntingLog>(created.Value);
            Assert.Equal(1, log.UserId);
            Assert.Equal("Szarvas", log.AnimalType);
            Assert.Equal(3, _context.HuntingLogs.Count());
        }

        [Fact]
        public async Task Create_AsSocietyAccount_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(5, societyAccount: true);
            var request = new HuntingLogsController.HuntingLogCreateRequest { OccurredAt = DateTime.UtcNow };

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(2, _context.HuntingLogs.Count());
        }

        [Fact]
        public async Task Update_AsOwner_UpdatesFieldsAndReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            var update = new HuntingLog
            {
                HuntingLogId = _ownLogId,
                OccurredAt = DateTime.UtcNow,
                AnimalType = "Roka",
                ShotFired = false,
                Notes = "Modositott bejegyzes"
            };

            // Act
            var result = await _controller.Update(_ownLogId, update);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var stored = _context.HuntingLogs.Single(h => h.HuntingLogId == _ownLogId);
            Assert.Equal("Roka", stored.AnimalType);
            Assert.Equal("Modositott bejegyzes", stored.Notes);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Update(_ownLogId, new HuntingLog { HuntingLogId = 9999 });

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Update_NonExistingLog_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Update(9999, new HuntingLog { HuntingLogId = 9999 });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OthersLog_ReturnsForbid()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Update(_otherLogId, new HuntingLog { HuntingLogId = _otherLogId });

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_AsOwner_ReturnsNoContentAndRemoves()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Delete(_ownLogId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.False(_context.HuntingLogs.Any(h => h.HuntingLogId == _ownLogId));
        }

        [Fact]
        public async Task Delete_OthersLog_ReturnsForbid()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Delete(_otherLogId);

            // Assert
            Assert.IsType<ForbidResult>(result);
            Assert.True(_context.HuntingLogs.Any(h => h.HuntingLogId == _otherLogId));
        }

        [Fact]
        public async Task Delete_NonExistingLog_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Delete(9999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
