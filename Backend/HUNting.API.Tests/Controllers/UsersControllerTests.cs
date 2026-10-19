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
using Microsoft.AspNetCore.Identity;
using HUNting.API.Controllers;
using HUNting.API.Services;
using HUNting.API.Models.Users;
using MockQueryable.Moq;

namespace HUNting.API.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new UsersController(_userServiceMock.Object, _userManagerMock.Object);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Administrator")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<ApplicationUser> 
            { 
                new ApplicationUser { Id = 1, FullName = "User 1", CreatedAt = DateTime.Now },
                new ApplicationUser { Id = 2, FullName = "User 2", CreatedAt = DateTime.Now }
            }.AsQueryable();

            var mockSet = users.BuildMock();
            _userManagerMock.Setup(m => m.Users).Returns(mockSet);
            _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var resultList = okResult.Value as List<UserResponse>;
            Assert.NotNull(resultList);
            Assert.Equal(2, resultList.Count);
        }

        [Fact]
        public async Task Get_ExistingUser_ReturnsOk()
        {
            // Arrange
            var user = new ApplicationUser { Id = 1, FullName = "Admin", CreatedAt = DateTime.Now };
            _userServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Administrator" });

            // Act
            var result = await _controller.Get(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as UserResponse;
            Assert.NotNull(response);
            Assert.Equal("Admin", response.FullName);
            Assert.Contains("Administrator", response.Roles);
        }

        [Fact]
        public async Task Get_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.Get(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ValidUser_ReturnsCreated()
        {
            // Arrange
            var request = new UserCreateRequest("New User", "new@test.com", "Password123!", null, new List<string> { "Hunter" });
            var createdUser = new ApplicationUser { Id = 3, FullName = "New User", Email = "new@test.com", CreatedAt = DateTime.Now };
            
            _userServiceMock.Setup(s => s.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!", request.Roles))
                            .ReturnsAsync(createdUser);
            _userManagerMock.Setup(m => m.GetRolesAsync(createdUser)).ReturnsAsync(new List<string> { "Hunter" });

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.NotNull(createdResult);
            Assert.Equal("Get", createdResult.ActionName);
            var response = createdResult.Value as UserResponse;
            Assert.NotNull(response);
            Assert.Equal(createdUser.Id, response.Id);
        }

        [Fact]
        public async Task Update_ValidUser_ReturnsNoContent()
        {
            // Arrange
            var request = new UserUpdateRequest(1, "Updated User", "up@test.com", null, new List<string> { "Hunter" });
            var existingUser = new ApplicationUser { Id = 1, FullName = "Old User", Email = "old@test.com" };
            
            _userServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _userManagerMock.Setup(m => m.GetRolesAsync(existingUser)).ReturnsAsync(new List<string> { "User" });
            
            // Assume add/remove roles succeeds
            _userManagerMock.Setup(m => m.RemoveFromRolesAsync(existingUser, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRolesAsync(existingUser, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Update(1, request);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _userServiceMock.Verify(s => s.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            _userServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _userServiceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserUpdateRequest(2, "Valaki Mas", "v@test.com", null, null);

            // Act
            var result = await _controller.Update(1, request);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _userServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task Update_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ApplicationUser?)null);
            var request = new UserUpdateRequest(99, "Senki", "s@test.com", null, null);

            // Act
            var result = await _controller.Update(99, request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
