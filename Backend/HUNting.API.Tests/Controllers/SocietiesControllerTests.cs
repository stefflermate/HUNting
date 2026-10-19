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
            
            var postReq = new SocietyPostCreateRequest("Uj hir", "Hir tartalma", null);
            var expectedPost = new SocietyPost { SocietyPostId = 11, SocietyId = 1, Title = "Uj hir" };
            
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

        [Fact]
        public async Task Get_ExistingId_ReturnsOk()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, Name = "Vadasztarsasag 1" });

            // Act
            var result = await _controller.Get(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var society = Assert.IsType<Society>(okResult.Value);
            Assert.Equal(1, society.SocietyId);
        }

        [Fact]
        public async Task Get_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Society?)null);

            // Act
            var result = await _controller.Get(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMySocieties_ReturnsUsersSocieties()
        {
            // Arrange
            SetUserContext(2);
            _serviceMock.Setup(s => s.GetByUserAsync(2)).ReturnsAsync(new List<Society> { new Society { SocietyId = 1 } });

            // Act
            var result = await _controller.GetMySocieties();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<Society>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task Members_AsOwner_ReturnsOk()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            _serviceMock.Setup(s => s.GetMembersAsync(1))
                        .ReturnsAsync(new List<SocietyMember> { new SocietyMember { SocietyId = 1, UserId = 2, RoleId = 1 } });

            // Act
            var result = await _controller.Members(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var members = Assert.IsAssignableFrom<IEnumerable<SocietyMember>>(okResult.Value);
            Assert.Single(members);
        }

        [Fact]
        public async Task Members_AsNonOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(3);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.Members(1);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task Members_NonExistingSociety_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Society?)null);

            // Act
            var result = await _controller.Members(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RequestMembership_AsHunter_ReturnsOk()
        {
            // Arrange
            SetUserContext(3);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            var created = new SocietyMembershipRequest { SocietyId = 1, UserId = 3, Message = "Szeretnek csatlakozni" };
            _serviceMock.Setup(s => s.CreateMembershipRequestAsync(1, 3, "Szeretnek csatlakozni")).ReturnsAsync(created);

            // Act
            var result = await _controller.RequestMembership(1, new MembershipRequestDto("Szeretnek csatlakozni"));

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(created, okResult.Value);
        }

        [Fact]
        public async Task RequestMembership_AsSocietyAccount_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(5, isSocietyAccount: true);

            // Act
            var result = await _controller.RequestMembership(1, new MembershipRequestDto(null));

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            _serviceMock.Verify(s => s.CreateMembershipRequestAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task RequestMembership_NonExistingSociety_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(3);
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Society?)null);

            // Act
            var result = await _controller.RequestMembership(99, new MembershipRequestDto(null));

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RequestMembership_DuplicateRequest_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(3);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            _serviceMock.Setup(s => s.CreateMembershipRequestAsync(1, 3, null))
                        .ThrowsAsync(new InvalidOperationException("Mar van folyamatban levo kerelem."));

            // Act
            var result = await _controller.RequestMembership(1, new MembershipRequestDto(null));

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetRequests_AsOwner_ReturnsOk()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            _serviceMock.Setup(s => s.GetMembershipRequestsAsync(1))
                        .ReturnsAsync(new List<SocietyMembershipRequest> { new SocietyMembershipRequest { SocietyId = 1, UserId = 3 } });

            // Act
            var result = await _controller.GetRequests(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var requests = Assert.IsAssignableFrom<IEnumerable<SocietyMembershipRequest>>(okResult.Value);
            Assert.Single(requests);
        }

        [Fact]
        public async Task GetRequests_AsNonOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(3);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.GetRequests(1);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task ApproveRequest_AsOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.ApproveRequest(1, 3);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.ApproveMembershipRequestAsync(1, 3, 1), Times.Once);
        }

        [Fact]
        public async Task ApproveRequest_AsNonOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(3);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.ApproveRequest(1, 3);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _serviceMock.Verify(s => s.ApproveMembershipRequestAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ApproveRequest_ServiceThrows_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });
            _serviceMock.Setup(s => s.ApproveMembershipRequestAsync(1, 3, 1))
                        .ThrowsAsync(new InvalidOperationException("Nincs fuggo kerelem."));

            // Act
            var result = await _controller.ApproveRequest(1, 3);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RejectRequest_AsOwner_ReturnsNoContent()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.RejectRequest(1, 3);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.RejectMembershipRequestAsync(1, 3, 1), Times.Once);
        }

        [Fact]
        public async Task GetPosts_AsAdmin_ReturnsOk()
        {
            // Arrange
            SetUserContext(99, role: "Administrator");
            _serviceMock.Setup(s => s.GetPostsAsync(1)).ReturnsAsync(new List<SocietyPost>());

            // Act
            var result = await _controller.GetPosts(1);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreatePost_AsNonOwner_ReturnsForbid()
        {
            // Arrange
            SetUserContext(2);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.CreatePost(1, new SocietyPostCreateRequest("Cim", "Tartalom", null));

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _serviceMock.Verify(s => s.AddPostAsync(It.IsAny<SocietyPost>()), Times.Never);
        }

        [Fact]
        public async Task CreatePost_NonExistingSociety_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Society?)null);

            // Act
            var result = await _controller.CreatePost(99, new SocietyPostCreateRequest("Cim", "Tartalom", null));

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeletePost_NonExistingPost_ReturnsNotFound()
        {
            // Arrange
            SetUserContext(1);
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Society { SocietyId = 1, OwnerUserId = 1 });

            // Act
            var result = await _controller.DeletePost(1, 999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
