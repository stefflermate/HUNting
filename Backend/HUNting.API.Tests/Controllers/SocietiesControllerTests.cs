using System;
using System.Collections.Generic;
using System.IO;
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
using HUNting.API.Services;
using HUNting.API.Models.Societies;

namespace HUNting.API.Tests.Controllers
{
    public class SocietiesControllerTests : IDisposable
    {
        private readonly Mock<ISocietyService> _serviceMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly HuntingContext _context;
        private readonly SocietiesController _controller;

        public SocietiesControllerTests()
        {
            _serviceMock = new Mock<ISocietyService>();
            _envMock = new Mock<IWebHostEnvironment>();

            var options = new DbContextOptionsBuilder<HuntingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new HuntingContext(options);
            _controller = new SocietiesController(_serviceMock.Object, _context, _envMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            _context.Societies.Add(new Society { SocietyId = 1, Name = "Vadasztarsasag 1", OwnerUserId = 1 });
            _context.SocietyMembers.Add(new SocietyMember { SocietyId = 1, UserId = 2 }); // Member user
            _context.SocietyPosts.Add(new SocietyPost { SocietyPostId = 10, SocietyId = 1, AuthorUserId = 1, Title = "Test Post", Content = "Test Content" });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetUserContext(int userId, bool isSocietyAccount = false, string role = "Hunter")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            if (isSocietyAccount)
            {
                claims.Add(new Claim("account_type", "Society"));
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Society> { new Society { SocietyId = 1 } });

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var resultList = okResult.Value as IEnumerable<Society>;
            Assert.Single(resultList);
        }

        [Fact]
        public async Task GetPosts_AsMember_ReturnsOk()
        {
            // Arrange
            SetUserContext(2); // User 2 is member
            var posts = new List<SocietyPost> { new SocietyPost { SocietyPostId = 10, Title = "Test" } };
            _serviceMock.Setup(s => s.GetPostsAsync(1)).ReturnsAsync(posts);

            // Act
            var result = await _controller.GetPosts(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var resultList = okResult.Value as IEnumerable<SocietyPost>;
            Assert.Single(resultList);
        }

        [Fact]
        public async Task GetPosts_AsNonMember_ReturnsForbid()
        {
            // Arrange
            SetUserContext(3); // User 3 is NOT a member and NOT owner

            // Act
            var result = await _controller.GetPosts(1);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task CreatePost_AsOwner_ReturnsCreated()
        {
            // Arrange
            SetUserContext(1); // User 1 is Owner
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            
            var postReq = new SocietyPostCreateRequest("Új hír", "Hír tartalma", null);
            var expectedPost = new SocietyPost { SocietyPostId = 11, SocietyId = 1, Title = "Új hír" };
            
            _serviceMock.Setup(s => s.AddPostAsync(It.IsAny<SocietyPost>())).ReturnsAsync(expectedPost);

            // Act
            var result = await _controller.CreatePost(1, postReq);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.NotNull(createdResult);
            Assert.Equal(expectedPost, createdResult.Value);
        }

        [Fact]
        public async Task DeletePost_AsOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1); // Owner
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            _serviceMock.Setup(s => s.RemovePostAsync(1, 10)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeletePost(1, 10);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
