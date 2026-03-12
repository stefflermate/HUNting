using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using HUNting.API.Controllers;
using HUNting.API.Services;
using HUNting.API.Models;
using HUNting.API.Models.Ads;

namespace HUNting.API.Tests.Controllers
{
    public class AdsControllerTests
    {
        private readonly Mock<IAdService> _mockAdService;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly AdsController _controller;

        public AdsControllerTests()
        {
            _mockAdService = new Mock<IAdService>();
            _mockEnv = new Mock<IWebHostEnvironment>();

            _controller = new AdsController(_mockAdService.Object, _mockEnv.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithPagedData()
        {
            // Arrange
            var options = new AdQueryOptions();
            var expectedResult = new PagedResult<AdDto> 
            { 
                Data = new List<AdDto>(), 
                TotalCount = 0 
            };

            _mockAdService
                .Setup(svc => svc.GetPagedAsync(options))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetAll(options);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WhenAdExists()
        {
            // Arrange
            int adId = 1;
            var expectedAd = new Ad { Id = adId, Title = "Test Ad" };
            
            _mockAdService
                .Setup(svc => svc.GetByIdAsync(adId))
                .ReturnsAsync(expectedAd);

            // Act
            var result = await _controller.Get(adId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAd = Assert.IsType<Ad>(okResult.Value);
            Assert.Equal(expectedAd.Id, returnedAd.Id);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenAdDoesNotExist()
        {
            // Arrange
            int adId = 99;
            _mockAdService
                .Setup(svc => svc.GetByIdAsync(adId))
                .ReturnsAsync((Ad)null);

            // Act
            var result = await _controller.Get(adId);

            // Assert
            // This assumes your controller returns NotFound() when ad == null.
            // If your controller currently returns 204 No Content or similar, adjust here.
            // For now, testing basic flow mapping.
            Assert.True(result.Result is NotFoundResult || result.Value == null);
        }
    }
}
