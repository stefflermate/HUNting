using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using HUNting.API.Controllers;
using HUNting.API.Services;
using HUNting.API.Models;
using HUNting.API.Models.Ads;

namespace HUNting.API.Tests.Controllers
{
    public class AdsControllerTests
    {
        private readonly Mock<IAdService> _adServiceMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly AdsController _controller;

        public AdsControllerTests()
        {
            _adServiceMock = new Mock<IAdService>();
            _envMock = new Mock<IWebHostEnvironment>();

            _controller = new AdsController(_adServiceMock.Object, _envMock.Object);
        }

        private void SetUserContext(int userId, string role = "User")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithPagedData()
        {
            // Arrange
            var options = new AdQueryOptions();
            // Assuming Ad is returned or AdDto, adapt as per your generic PagedResult
            var pagedResult = new PagedResult<Ad>(new List<Ad> { new Ad { AdId = 1, Title = "Hirdetes" } }, 1, 1, 10);
            _adServiceMock.Setup(s => s.GetPagedAsync(options)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAll(options) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(pagedResult, result.Value);
        }

        [Fact]
        public async Task Get_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var ad = new Ad { AdId = 10, Title = "Puska" };
            _adServiceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(ad);

            // Act
            var result = await _controller.Get(10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(ad, okResult.Value);
        }

        [Fact]
        public async Task Get_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _adServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Ad)null);

            // Act
            var result = await _controller.Get(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ValidAd_ReturnsCreated()
        {
            // Arrange
            SetUserContext(1);
            var newAd = new Ad { Title = "Új Hirdetés", Price = 50000 };

            // Act
            var result = await _controller.Create(newAd);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.NotNull(createdResult);
            Assert.Equal("Get", createdResult.ActionName);
            Assert.Equal(1, newAd.OwnerUserId); // Owner is set by the controller based on user context
        }

        [Fact]
        public async Task Update_ValidAdAsOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            var existingAd = new Ad { AdId = 5, OwnerUserId = 1, Title = "Old Title" };
            var updateAd = new Ad { AdId = 5, Title = "New Title" };

            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(existingAd);

            // Act
            var result = await _controller.Update(5, updateAd);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _adServiceMock.Verify(s => s.UpdateAsync(It.IsAny<Ad>()), Times.Once);
        }

        [Fact]
        public async Task Update_UpdateOtherUserAd_WithoutAdmin_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2); // Logged in as User 2
            var existingAd = new Ad { AdId = 5, OwnerUserId = 1, Title = "Old" }; // Belongs to User 1
            var updateAd = new Ad { AdId = 5, Title = "New" };

            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(existingAd);

            // Act
            var result = await _controller.Update(5, updateAd);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _adServiceMock.Verify(s => s.UpdateAsync(It.IsAny<Ad>()), Times.Never);
        }

        [Fact]
        public async Task Delete_AsOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            var existingAd = new Ad { AdId = 5, OwnerUserId = 1 };
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(existingAd);

            // Act
            var result = await _controller.Delete(5);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _adServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
        }

        private static IFormFile CreateFormFile(string fileName, long length = 100)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(length);
            return fileMock.Object;
        }

        [Fact]
        public async Task Categories_ReturnsOkWithList()
        {
            // Arrange
            _adServiceMock.Setup(s => s.GetCategoriesAsync())
                          .ReturnsAsync(new List<AdCategory> { new AdCategory { CategoryId = 1, Name = "Fegyver" } });

            // Act
            var result = await _controller.Categories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categories = Assert.IsAssignableFrom<IEnumerable<AdCategory>>(okResult.Value);
            Assert.Single(categories);
        }

        [Fact]
        public async Task Statuses_ReturnsOkWithList()
        {
            // Arrange
            _adServiceMock.Setup(s => s.GetStatusesAsync())
                          .ReturnsAsync(new List<AdStatus> { new AdStatus { StatusCode = "ACTIVE" } });

            // Act
            var result = await _controller.Statuses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var statuses = Assert.IsAssignableFrom<IEnumerable<AdStatus>>(okResult.Value);
            Assert.Single(statuses);
        }

        [Fact]
        public async Task Create_ServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.AddAsync(It.IsAny<Ad>())).ThrowsAsync(new ArgumentException("Ervenytelen kategoria."));

            // Act
            var result = await _controller.Create(new Ad { Title = "Hibas hirdetes" });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_AsAdmin_KeepsProvidedOwner()
        {
            // Arrange
            SetUserContext(10, "Administrator");
            var ad = new Ad { Title = "Admin hirdetes", OwnerUserId = 5 };

            // Act
            var result = await _controller.Create(ad);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(5, ad.OwnerUserId);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.Update(1, new Ad { AdId = 2 });

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Update_NonExistingAd_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Ad?)null);

            // Act
            var result = await _controller.Update(99, new Ad { AdId = 99 });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_AsAdmin_CanReassignOwner()
        {
            // Arrange
            SetUserContext(10, "Administrator");
            var existing = new Ad { AdId = 5, OwnerUserId = 1 };
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(existing);
            var update = new Ad { AdId = 5, Title = "Uj cim", OwnerUserId = null, OwnerSocietyId = 3 };

            // Act
            var result = await _controller.Update(5, update);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(existing.OwnerUserId);
            Assert.Equal(3, existing.OwnerSocietyId);
        }

        [Fact]
        public async Task Delete_NonExistingAd_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Ad?)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_OtherUsersAd_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new Ad { AdId = 5, OwnerUserId = 1 });

            // Act
            var result = await _controller.Delete(5);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _adServiceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UploadImage_MissingFile_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.UploadImage(5, null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UploadImage_NonExistingAd_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Ad?)null);

            // Act
            var result = await _controller.UploadImage(99, CreateFormFile("kep.jpg"));

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UploadImage_NotOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new Ad { AdId = 5, OwnerUserId = 1 });

            // Act
            var result = await _controller.UploadImage(5, CreateFormFile("kep.jpg"));

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task UploadImage_InvalidExtension_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new Ad { AdId = 5, OwnerUserId = 1 });

            // Act
            var result = await _controller.UploadImage(5, CreateFormFile("virus.exe"));

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            _adServiceMock.Verify(s => s.AddImageAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task DeleteImage_NonExistingAd_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Ad?)null);

            // Act
            var result = await _controller.DeleteImage(99, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteImage_ImageBelongsToOtherAd_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new Ad { AdId = 5, OwnerUserId = 1 });
            _adServiceMock.Setup(s => s.GetImageByIdAsync(7)).ReturnsAsync(new AdImage { ImageId = 7, AdId = 999, ImageUrl = "/uploads/ads/x.jpg" });

            // Act
            var result = await _controller.DeleteImage(5, 7);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _adServiceMock.Verify(s => s.RemoveImageAsync(It.IsAny<AdImage>()), Times.Never);
        }

        [Fact]
        public async Task DeleteImage_AsOwner_RemovesImageAndReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            var image = new AdImage { ImageId = 7, AdId = 5, ImageUrl = "/uploads/ads/torlendo.jpg" };
            _adServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new Ad { AdId = 5, OwnerUserId = 1 });
            _adServiceMock.Setup(s => s.GetImageByIdAsync(7)).ReturnsAsync(image);

            // Act
            var result = await _controller.DeleteImage(5, 7);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _adServiceMock.Verify(s => s.RemoveImageAsync(image), Times.Once);
        }
    }
}
