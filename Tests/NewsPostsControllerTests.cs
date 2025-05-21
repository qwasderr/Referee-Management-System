using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using Xunit;
namespace Tests;
public class NewsPostsControllerTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return context;
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewsPosts()
    {
        var context = GetInMemoryDbContext();
        context.NewsPosts.Add(new NewsPost { NewsPostId = 1, Title = "Test 1", Content = "Content 1", CreatedAt = DateTime.Now });
        context.NewsPosts.Add(new NewsPost { NewsPostId = 2, Title = "Test 2", Content = "Content 2", CreatedAt = DateTime.Now });
        await context.SaveChangesAsync();

        var controller = new NewsPostsController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.Generic.List<NewsPost>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetInMemoryDbContext();
        var controller = new NewsPostsController(context);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenNewsPostNotFound()
    {
        var context = GetInMemoryDbContext();
        var controller = new NewsPostsController(context);

        var result = await controller.Details(123);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewResult_WithNewsPost()
    {
        var context = GetInMemoryDbContext();
        var newsPost = new NewsPost { NewsPostId = 1, Title = "Title", Content = "Content", CreatedAt = DateTime.Now };
        context.NewsPosts.Add(newsPost);
        await context.SaveChangesAsync();

        var controller = new NewsPostsController(context);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewsPost>(viewResult.Model);
        Assert.Equal(newsPost.Title, model.Title);
    }

    [Fact]
    public async Task Create_Post_ReturnsRedirect_WhenModelStateValid()
    {
        var context = GetInMemoryDbContext();
        var controller = new NewsPostsController(context);

        var newsPost = new NewsPost { Title = "New Post", Content = "Content" };

        var result = await controller.Create(newsPost, null);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        Assert.Equal(1, context.NewsPosts.Count());
    }

    [Fact]
    public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
    {
        var context = GetInMemoryDbContext();
        var controller = new NewsPostsController(context);

        var newsPost = new NewsPost { NewsPostId = 1, Title = "Title", Content = "Content" };

        var result = await controller.Edit(2, newsPost, null);

        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task DeleteConfirmed_RemovesNewsPost_AndRedirects()
    {
        var context = GetInMemoryDbContext();
        var newsPost = new NewsPost { NewsPostId = 1, Title = "Title", Content = "Content", CreatedAt = DateTime.Now };
        context.NewsPosts.Add(newsPost);
        await context.SaveChangesAsync();

        var controller = new NewsPostsController(context);

        var result = await controller.DeleteConfirmed(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Empty(context.NewsPosts);
    }
}
