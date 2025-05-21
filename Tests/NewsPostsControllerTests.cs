using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;

namespace Tests
{
    public class NewsPostsControllerTests
    {
        private class FakeImageService : IImageService
        {
            public Task<string?> SaveImageAsync(IFormFile image, string folderPath)
            {
                return Task.FromResult<string?>("/images/news/test.jpg");
            }

            public void DeleteImage(string relativePath, string folderPath)
            {
            }
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfNewsPosts()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            context.NewsPosts.Add(new NewsPost { NewsPostId = 1, Title = "Test 1", Content = "Content 1", CreatedAt = DateTime.Now });
            context.NewsPosts.Add(new NewsPost { NewsPostId = 2, Title = "Test 2", Content = "Content 2", CreatedAt = DateTime.Now });
            await context.SaveChangesAsync();

            var controller = new NewsPostsController(context, imageService);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<NewsPost>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            var controller = new NewsPostsController(context, imageService);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenNewsPostNotFound()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            var controller = new NewsPostsController(context, imageService);

            var result = await controller.Details(123);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithNewsPost()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            var newsPost = new NewsPost { NewsPostId = 1, Title = "Title", Content = "Content", CreatedAt = DateTime.Now };
            context.NewsPosts.Add(newsPost);
            await context.SaveChangesAsync();

            var controller = new NewsPostsController(context, imageService);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NewsPost>(viewResult.Model);
            Assert.Equal(newsPost.Title, model.Title);
        }

        [Fact]
        public async Task Create_Post_WithPhoto_SavesPhotoAndRedirects()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            var controller = new NewsPostsController(context, imageService);

            var newsPost = new NewsPost { Title = "New Post", Content = "Content" };

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);

            var result = await controller.Create(newsPost, fileMock.Object);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var savedPost = context.NewsPosts.FirstOrDefault();
            Assert.NotNull(savedPost);
            Assert.Equal("/images/news/test.jpg", savedPost.PhotoPath);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            var controller = new NewsPostsController(context, imageService);

            var newsPost = new NewsPost { NewsPostId = 1, Title = "Title", Content = "Content" };

            var result = await controller.Edit(2, newsPost, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesNewsPost_AndRedirects()
        {
            var context = GetInMemoryDbContext();
            var imageService = new FakeImageService();
            var newsPost = new NewsPost { NewsPostId = 1, Title = "Title", Content = "Content", CreatedAt = DateTime.Now };
            context.NewsPosts.Add(newsPost);
            await context.SaveChangesAsync();

            var controller = new NewsPostsController(context, imageService);

            var result = await controller.DeleteConfirmed(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Empty(context.NewsPosts);
        }
    }
}
