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
        private readonly HuntingContext _dbContext;
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

            var options = new DbContextOptionsBuilder<HuntingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new HuntingContext(options);

            _controller = new AuthController(
                _userManagerMock.Object, 
                _signInManagerMock.Object, 
                _tokenServiceMock.Object,
                _dbContext);
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

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var req = new LoginRequest("test@test.com", "RosszJelszo1!");
            var user = new ApplicationUser { Id = 1, Email = req.Email, FullName = "Test Elek" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(req.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, req.Password, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(req);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        private void SetUserContext(int? userId)
        {
            var identity = userId.HasValue
                ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }, "mock")
                : new ClaimsIdentity();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private void SetupSuccessfulUserCreation(int newUserId, string expectedRole)
        {
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                            .Callback<ApplicationUser, string>((u, _) => u.Id = newUserId)
                            .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), expectedRole))
                            .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(new List<string> { expectedRole });
            _tokenServiceMock.Setup(s => s.CreateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
                             .Returns(new TokenResult("token-xyz", DateTime.UtcNow.AddHours(1)));
        }

        [Fact]
        public async Task Register_PersonAccount_ReturnsAuthResponse()
        {
            // Arrange
            SetupSuccessfulUserCreation(10, "Hunter");
            var req = new RegisterRequest("Teszt Elek", "uj@test.com", "Password123!", "+36301234567");

            // Act
            var result = await _controller.Register(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal("token-xyz", response.AccessToken);
            Assert.Equal("Teszt Elek", response.User.Name);
            Assert.Null(response.User.OwnedSociety);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Hunter"), Times.Once);
        }

        [Fact]
        public async Task Register_CreateFails_ReturnsBadRequest()
        {
            // Arrange
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Gyenge jelszo" }));
            var req = new RegisterRequest("Teszt Elek", "uj@test.com", "1", null);

            // Act
            var result = await _controller.Register(req);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            _tokenServiceMock.Verify(s => s.CreateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Register_SocietyAccount_WithoutSocietyName_ReturnsBadRequest()
        {
            // Arrange
            SetupSuccessfulUserCreation(11, "SocietyManager");
            var req = new RegisterRequest("Tarsasag Vezeto", "tars@test.com", "Password123!", null, AccountType.Society, null);

            // Act
            var result = await _controller.Register(req);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Empty(_dbContext.Societies.ToList());
        }

        [Fact]
        public async Task Register_SocietyAccount_CreatesSocietyAndMembership()
        {
            // Arrange
            _dbContext.SocietyRoles.Add(new SocietyRole { RoleId = 1, Name = "Vezeto" });
            await _dbContext.SaveChangesAsync();

            SetupSuccessfulUserCreation(20, "SocietyManager");
            var req = new RegisterRequest(
                "Tarsasag Vezeto", "tars@test.com", "Password123!", null,
                AccountType.Society, new SocietyRegistrationRequest("Hubertus Vt", null, null, "Leiras"));

            // Act
            var result = await _controller.Register(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.NotNull(response.User.OwnedSociety);
            Assert.Equal("Hubertus Vt", response.User.OwnedSociety!.Name);

            var society = Assert.Single(_dbContext.Societies.ToList());
            Assert.Equal(20, society.OwnerUserId);
            var member = Assert.Single(_dbContext.SocietyMembers.ToList());
            Assert.Equal(20, member.UserId);
            Assert.Equal(1, member.RoleId);
        }

        [Fact]
        public void Logout_ReturnsNoContent()
        {
            // Act
            var result = _controller.Logout();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Me_AuthenticatedUser_ReturnsUserDto()
        {
            // Arrange
            SetUserContext(7);
            var user = new ApplicationUser { Id = 7, Email = "me@test.com", FullName = "Sajat Profil" };
            _userManagerMock.Setup(m => m.FindByIdAsync("7")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Hunter" });

            // Act
            var result = await _controller.Me();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(7, dto.Id);
            Assert.Equal("Sajat Profil", dto.Name);
        }

        [Fact]
        public async Task Me_MissingClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetUserContext(null);

            // Act
            var result = await _controller.Me();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task Me_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            SetUserContext(8);
            _userManagerMock.Setup(m => m.FindByIdAsync("8")).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Me();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }
    }
}
