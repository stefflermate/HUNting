using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using HUNting.API.Controllers;
using HUNting.API.Services.Auth;
using HUNting.API.Models.Users;
using HUNting.API.Models.Societies;
using HUNting.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null, null, null, null);

            _tokenServiceMock = new Mock<ITokenService>();

            // Mock Data layer (DbContext) lightly for basic flow. Since we rely on a real db context in AuthController (which is not purely isolated),
            // we will create an in-memory context just for the test if possible, or use a mocked context if possible.
            // A quick fix is to use an in-memory DbContext options
            var options = new DbContextOptionsBuilder<HuntingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new HuntingContext(options);

            _controller = new AuthController(
                _userManagerMock.Object, 
                _signInManagerMock.Object, 
                _tokenServiceMock.Object,
                dbContext);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var req = new LoginRequest("test@test.com", "Password123!");
            var user = new ApplicationUser { Id = 1, Email = "test@test.com", FullName = "Test Elek" };

            _userManagerMock.Setup(m => m.FindByEmailAsync(req.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, req.Password, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Hunter" });
            
            _tokenServiceMock.Setup(s => s.CreateToken(user, It.IsAny<IList<string>>()))
                             .Returns(new TokenResult("token123", DateTime.Now.AddHours(1)));

            // Act
            var result = await _controller.Login(req);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as AuthResponse;
            Assert.NotNull(response);
            Assert.Equal("token123", response.AccessToken);
            Assert.Equal("Test Elek", response.User.Name);
        }

        [Fact]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            var req = new LoginRequest("bad@test.com", "assword123!");
            _userManagerMock.Setup(m => m.FindByEmailAsync(req.Email)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.Login(req);

            // Assert
            var unauthorized = result.Result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorized);
        }
    }
}
