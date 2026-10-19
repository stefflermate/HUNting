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

        private void SetAnonymousContext()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task GetMarkers_PersonalScope_WithoutUser_ReturnsUnauthorized()
        {
            // Arrange
            SetAnonymousContext();
            var query = new MapController.MapLayerQuery("personal", null);

            // Act
            var result = await _controller.GetMarkers(query, CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetMarkers_SocietyScope_WithoutSocietyId_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);
            var query = new MapController.MapLayerQuery("society", null);

            // Act
            var result = await _controller.GetMarkers(query, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMarkers_SocietyScope_NotMember_ReturnsForbid()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.UserBelongsToSocietyAsync(2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var query = new MapController.MapLayerQuery("society", 2);

            // Act
            var result = await _controller.GetMarkers(query, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetMarkers_SocietyScope_AsMember_ReturnsMarkers()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.UserBelongsToSocietyAsync(2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mapServiceMock.Setup(s => s.GetMarkersAsync(null, 2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<MapMarker> { new MapMarker { MarkerId = 3, SocietyId = 2 } });
            var query = new MapController.MapLayerQuery("society", 2);

            // Act
            var result = await _controller.GetMarkers(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var markers = Assert.IsAssignableFrom<IEnumerable<MapController.MapMarkerResponse>>(okResult.Value);
            Assert.Single(markers);
        }

        [Fact]
        public async Task GetMarkers_UnknownScope_ReturnsEmptyList()
        {
            // Arrange
            SetAnonymousContext();
            var query = new MapController.MapLayerQuery(null, null);

            // Act
            var result = await _controller.GetMarkers(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var markers = Assert.IsAssignableFrom<IEnumerable<MapController.MapMarkerResponse>>(okResult.Value);
            Assert.Empty(markers);
        }

        [Fact]
        public async Task CreateMarker_WithoutUser_ReturnsUnauthorized()
        {
            // Arrange
            SetAnonymousContext();
            var request = new MapController.MapMarkerRequest(1, 47.0M, 19.0M, null, null, null);

            // Act
            var result = await _controller.CreateMarker(request, CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task CreateMarker_ForeignSociety_ReturnsForbid()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.UserBelongsToSocietyAsync(5, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var request = new MapController.MapMarkerRequest(1, 47.0M, 19.0M, null, "SOCIETY", 5);

            // Act
            var result = await _controller.CreateMarker(request, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _mapServiceMock.Verify(s => s.CreateMarkerAsync(It.IsAny<MapMarker>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMarker_AsOwner_ReturnsOk()
        {
            // Arrange
            SetUserContext(1);
            var updated = new MapMarker { MarkerId = 5, CreatedByUserId = 1, IconTypeId = 2, Latitude = 47.5M, Longitude = 19.1M };
            _mapServiceMock.Setup(s => s.UpdateMarkerAsync(It.IsAny<MapMarker>(), 1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(updated);
            var request = new MapController.MapMarkerRequest(2, 47.5M, 19.1M, "Modositott", "PRIVATE", null);

            // Act
            var result = await _controller.UpdateMarker(5, request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<MapController.MapMarkerResponse>(okResult.Value);
            Assert.Equal(5, response.MarkerId);
        }

        [Fact]
        public async Task UpdateMarker_NonExisting_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.UpdateMarkerAsync(It.IsAny<MapMarker>(), 1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((MapMarker?)null);
            var request = new MapController.MapMarkerRequest(1, 47.0M, 19.0M, null, null, null);

            // Act
            var result = await _controller.UpdateMarker(99, request, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateMarker_NotOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _mapServiceMock.Setup(s => s.UpdateMarkerAsync(It.IsAny<MapMarker>(), 2, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new UnauthorizedAccessException());
            var request = new MapController.MapMarkerRequest(1, 47.0M, 19.0M, null, null, null);

            // Act
            var result = await _controller.UpdateMarker(5, request, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetPolygons_PersonalScope_ReturnsPolygons()
        {
            // Arrange
            SetUserContext(1);
            var polygons = new List<MapPolygon>
            {
                new MapPolygon
                {
                    PolygonId = 7,
                    CreatedByUserId = 1,
                    Points = new List<MapPolygonPoint>
                    {
                        new MapPolygonPoint { Seq = 0, Latitude = 47.0M, Longitude = 19.0M },
                        new MapPolygonPoint { Seq = 1, Latitude = 47.1M, Longitude = 19.1M }
                    }
                }
            };
            _mapServiceMock.Setup(s => s.GetPolygonsAsync(1, null, It.IsAny<CancellationToken>())).ReturnsAsync(polygons);
            var query = new MapController.MapLayerQuery("personal", null);

            // Act
            var result = await _controller.GetPolygons(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var responses = Assert.IsAssignableFrom<IEnumerable<MapController.MapPolygonResponse>>(okResult.Value);
            var polygon = Assert.Single(responses);
            Assert.Equal(7, polygon.PolygonId);
            Assert.Equal(2, polygon.Points.Count());
        }

        [Fact]
        public async Task GetPolygons_SocietyScope_WithoutSocietyId_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);
            var query = new MapController.MapLayerQuery("society", null);

            // Act
            var result = await _controller.GetPolygons(query, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreatePolygon_AuthorizedUser_ReturnsCreated()
        {
            // Arrange
            SetUserContext(1);
            var created = new MapPolygon { PolygonId = 8, CreatedByUserId = 1, Name = "Vadaszterulet" };
            _mapServiceMock.Setup(s => s.CreatePolygonAsync(It.IsAny<MapPolygon>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(created);
            var request = new MapController.MapPolygonRequest(
                "Vadaszterulet", null, "PRIVATE", "#ff0000", "#00ff00", null,
                new List<MapController.MapPointRequest>
                {
                    new MapController.MapPointRequest(47.0M, 19.0M),
                    new MapController.MapPointRequest(47.1M, 19.1M)
                });

            // Act
            var result = await _controller.CreatePolygon(request, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<MapController.MapPolygonResponse>(createdResult.Value);
            Assert.Equal(8, response.PolygonId);
        }

        [Fact]
        public async Task CreatePolygon_ForeignSociety_ReturnsForbid()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.UserBelongsToSocietyAsync(9, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var request = new MapController.MapPolygonRequest(null, null, null, null, null, 9, new List<MapController.MapPointRequest>());

            // Act
            var result = await _controller.CreatePolygon(request, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _mapServiceMock.Verify(s => s.CreatePolygonAsync(It.IsAny<MapPolygon>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePolygon_AsOwner_ReturnsOk()
        {
            // Arrange
            SetUserContext(1);
            var updated = new MapPolygon { PolygonId = 7, CreatedByUserId = 1, Name = "Modositott" };
            _mapServiceMock.Setup(s => s.UpdatePolygonAsync(It.IsAny<MapPolygon>(), 1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(updated);
            var request = new MapController.MapPolygonRequest("Modositott", null, null, null, null, null, new List<MapController.MapPointRequest>());

            // Act
            var result = await _controller.UpdatePolygon(7, request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<MapController.MapPolygonResponse>(okResult.Value);
            Assert.Equal("Modositott", response.Name);
        }

        [Fact]
        public async Task UpdatePolygon_NonExisting_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _mapServiceMock.Setup(s => s.UpdatePolygonAsync(It.IsAny<MapPolygon>(), 1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((MapPolygon?)null);
            var request = new MapController.MapPolygonRequest(null, null, null, null, null, null, new List<MapController.MapPointRequest>());

            // Act
            var result = await _controller.UpdatePolygon(99, request, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdatePolygon_NotOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _mapServiceMock.Setup(s => s.UpdatePolygonAsync(It.IsAny<MapPolygon>(), 2, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new UnauthorizedAccessException());
            var request = new MapController.MapPolygonRequest(null, null, null, null, null, null, new List<MapController.MapPointRequest>());

            // Act
            var result = await _controller.UpdatePolygon(7, request, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task DeletePolygon_AsOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);

            // Act
            var result = await _controller.DeletePolygon(7, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mapServiceMock.Verify(s => s.DeletePolygonAsync(7, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePolygon_NotOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _mapServiceMock.Setup(s => s.DeletePolygonAsync(7, 2, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.DeletePolygon(7, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
