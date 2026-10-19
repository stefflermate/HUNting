using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HUNting.API.Controllers;
using HUNting.API.Data;
using HUNting.API.Models.News;
using HUNting.API.Models.Users;
using HUNting.API.Models.Societies;

namespace HUNting.API.Tests.Controllers
{
    public class NewsControllerTests
    {
        private DbContextOptions<HuntingContext> GetOptions(string dbName)
        {
            return new DbContextOptionsBuilder<HuntingContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        private async Task BuildSeedDataAsync(HuntingContext ctx)
        {
            if (!ctx.Users.Any())
            {
                ctx.Users.Add(new ApplicationUser { Id = 1, UserName = "testuser", Email = "test@test.com", PasswordHash = "hash", FullName = "Test User" });
            }
            if (!ctx.Societies.Any())
            {
                ctx.Societies.Add(new Society { SocietyId = 1, Name = "Society1", Email = "soc@soc.com", PasswordHash = "hash" });
            }
            if (!ctx.NewsVisibility.Any())
            {
                ctx.NewsVisibility.Add(new NewsVisibility { VisibilityCode = "MEMBERS", Description = "Members" });
            }
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithAllNews()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                await BuildSeedDataAsync(ctx);
                ctx.NewsFeed.AddRange(
                    new NewsFeed { Title = "News 1", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" },
                    new NewsFeed { Title = "News 2", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" }
                );
                await ctx.SaveChangesAsync();
            }

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                var controller = new NewsController(ctx);
                var result = await controller.GetAll();

                var okResult = result.Result as OkObjectResult;
                Assert.NotNull(okResult);
                var list = okResult.Value as IEnumerable<NewsFeed>;
                Assert.NotNull(list);
                Assert.Equal(2, list.Count());
            }
        }

        [Fact]
        public async Task Get_ExistingId_ReturnsOk()
        {
            var dbName = Guid.NewGuid().ToString();
            int newsId = 0;

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                await BuildSeedDataAsync(ctx);
                var news = new NewsFeed { Title = "News 1", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" };
                ctx.NewsFeed.Add(news);
                await ctx.SaveChangesAsync();
                newsId = news.NewsFeedId;
            }

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                var controller = new NewsController(ctx);
                var result = await controller.Get(newsId);

                var okResult = result.Result as OkObjectResult;
                Assert.NotNull(okResult);
                var returnedNews = okResult.Value as NewsFeed;
                Assert.NotNull(returnedNews);
                Assert.Equal("News 1", returnedNews.Title);
            }
        }

        [Fact]
        public async Task Create_ValidNews_ReturnsCreated()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = new HuntingContext(GetOptions(dbName));
            await BuildSeedDataAsync(ctx);
            var controller = new NewsController(ctx);
            var news = new NewsFeed { Title = "New post", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" };

            var result = await controller.Create(news);

            var created = result.Result as CreatedAtActionResult;
            Assert.NotNull(created);
            var returnedNews = created.Value as NewsFeed;
            Assert.NotNull(returnedNews);
            Assert.Equal("New post", returnedNews.Title);
            Assert.True(returnedNews.NewsFeedId > 0);
        }

        [Fact]
        public async Task Update_ValidNews_ReturnsNoContent()
        {
            var dbName = Guid.NewGuid().ToString();
            int newsId = 0;

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                await BuildSeedDataAsync(ctx);
                var news = new NewsFeed { Title = "Initial Post", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" };
                ctx.NewsFeed.Add(news);
                await ctx.SaveChangesAsync();
                newsId = news.NewsFeedId;
            }

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                var controller = new NewsController(ctx);
                var updatedNews = new NewsFeed { NewsFeedId = newsId, Title = "Updated Post", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" };
                var result = await controller.Update(newsId, updatedNews);
                Assert.IsType<NoContentResult>(result);
            }

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                var dbNews = await ctx.NewsFeed.FindAsync(newsId);
                Assert.Equal("Updated Post", dbNews.Title);
            }
        }

        [Fact]
        public async Task Delete_ExistingNews_ReturnsNoContent()
        {
            var dbName = Guid.NewGuid().ToString();
            int newsId = 0;

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                await BuildSeedDataAsync(ctx);
                var news = new NewsFeed { Title = "To Delete", AuthorUserId = 1, SocietyId = 1, VisibilityCode = "MEMBERS" };
                ctx.NewsFeed.Add(news);
                await ctx.SaveChangesAsync();
                newsId = news.NewsFeedId;
            }

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                var controller = new NewsController(ctx);
                var result = await controller.Delete(newsId);
                Assert.IsType<NoContentResult>(result);
            }

            using (var ctx = new HuntingContext(GetOptions(dbName)))
            {
                var dbNews = await ctx.NewsFeed.FindAsync(newsId);
                Assert.Null(dbNews);
            }
        }

        [Fact]
        public async Task Get_NonExistingId_ReturnsNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = new HuntingContext(GetOptions(dbName));
            var controller = new NewsController(ctx);

            var result = await controller.Get(12345);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = new HuntingContext(GetOptions(dbName));
            var controller = new NewsController(ctx);
            var news = new NewsFeed { NewsFeedId = 2, Title = "Masik hir" };

            var result = await controller.Update(1, news);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingNews_ReturnsNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = new HuntingContext(GetOptions(dbName));
            var controller = new NewsController(ctx);

            var result = await controller.Delete(12345);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
