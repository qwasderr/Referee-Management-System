using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using System.Security.Claims;
namespace Tests;
public class TestsControllerTests
{
    private ApplicationDbContext GetContextWithData()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Tests.Add(new Test { TestId = 1, Title = "Test 1", TestUrl = "http://example.com/test1" });

        context.Judges.Add(new Judge
        {
            JudgeId = 1,
            ApplicationUserId = "user1",
            FullName = "Test Judge",
            QualificationLevel = "Level 1"
        });

        context.SaveChanges();

        return context;
    }

    private TestsController GetControllerWithUser(ApplicationDbContext context, string userId)
    {
        var controller = new TestsController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task MarkAsCompleted_AddsTestResult_AndRedirects()
    {
        var context = GetContextWithData();
        var controller = GetControllerWithUser(context, "user1");

        var result = await controller.MarkAsCompleted(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var testResult = await context.TestResults.FirstOrDefaultAsync(tr => tr.TestId == 1 && tr.JudgeId == 1);
        Assert.NotNull(testResult);
    }

    [Fact]
    public async Task Index_ReturnsViewWithModel_ForJudge()
    {
        var context = GetContextWithData();

        context.TestResults.Add(new TestResult { TestResultId = 1, JudgeId = 1, TestId = 1, DateTaken = DateTime.Now });
        await context.SaveChangesAsync();

        var controller = GetControllerWithUser(context, "user1");

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.Generic.List<TestWithStatusViewModel>>(viewResult.Model);
        Assert.Single(model);
        Assert.True(model[0].IsCompleted);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_ForNullId()
    {
        var context = GetContextWithData();
        var controller = new TestsController(context);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_ForInvalidId()
    {
        var context = GetContextWithData();
        var controller = new TestsController(context);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsView_ForValidId()
    {
        var context = GetContextWithData();
        var controller = new TestsController(context);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Test>(viewResult.Model);
        Assert.Equal(1, model.TestId);
    }
}
