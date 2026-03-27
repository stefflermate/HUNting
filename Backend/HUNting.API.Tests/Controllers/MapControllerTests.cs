using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HUNting.API.Controllers;
using HUNting.API.Services;
using HUNting.API.Models.Map;

namespace HUNting.API.Tests.Controllers
{
    public class MapControllerTests
    {
        private readonly Mock<IMapService> _mapServiceMock;
        private readonly MapController _controller;

        public MapControllerTests()
        {
            _mapServiceMock = new Mock<IMapService>();
            _controller = new MapController(_mapServiceMock.Object);
        }

        private void SetUserContext(int userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Hunter")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetConfig_ReturnsConfigObjects()
        {
            // Arrange
            _mapServiceMock.Setup(s => s.GetIconTypesAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<MapIconType> { new MapIconType { IconTypeId = 1, Name = "TestIcon" } });
            _mapServiceMock.Setup(s => s.GetVisibilitiesAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<MapVisibility> { new MapVisibility { VisibilityCode = "PUBLIC" } });

            // Act
            var result = await _controller.GetConfig(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetMarkers_PersonalScope_ReturnsPersonalMarkers()
        {
            // Arrange
            SetUserContext(1);
            var query = new MapController.MapLayerQuery("personal", null);
            var markers = new List<MapMarker> { new MapMarker { MarkerId = 10, CreatedByUserId = 1, Latitude = 47.0M, Longitude = 19.0M, IconType = new MapIconType() } };
            
            _mapServiceMock.Setup(s => s.GetMarkersAsync(1, null, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(markers);

            // Act
            var result = await _controller.GetMarkers(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultList = okResult.Value as IEnumerable<MapController.MapMarkerResponse>;
            Assert.Single(resultList);
        }

        [Fact]
        public async Task CreateMarker_AuthorizedUser_ReturnsCreated()
        {
            // Arrange
            SetUserContext(1);
            var request = new MapController.MapMarkerRequest(1, 47.0M, 19.0M, null, "PRIVATE", null);
            var createdEntity = new MapMarker { MarkerId = 5, CreatedByUserId = 1, Latitude = 47.0M, Longitude = 19.0M, IconType = new MapIconType() };

            _mapServiceMock.Setup(s => s.CreateMarkerAsync(It.IsAny<MapMarker>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(createdEntity);

            // Act
            var result = await _controller.CreateMarker(request, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = createdResult.Value as MapController.MapMarkerResponse;
            Assert.Equal(5, response.MarkerId);
        }

        [Fact]
        public async Task DeleteMarker_AuthorizedOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.DeleteMarkerAsync(5, 1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteMarker(5, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        
        [Fact]
        public async Task DeleteMarker_NotOwner_ThrowsExceptionAndReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _mapServiceMock.Setup(s => s.DeleteMarkerAsync(5, 2, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.DeleteMarker(5, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
