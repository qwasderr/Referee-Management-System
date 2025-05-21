using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
namespace Tests;
public class TestResultsControllerTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        context.Judges.Add(new Judge { JudgeId = 1, FullName = "Judge 1", ApplicationUserId = "user1", QualificationLevel = "Level 1" });
        context.Tests.Add(new Test { TestId = 1, Title = "Test 1", TestUrl = "http://test1" });
        context.TestResults.Add(new TestResult { TestResultId = 1, JudgeId = 1, TestId = 1, DateTaken = DateTime.UtcNow, Score = null, Explanation = null });
        context.SaveChanges();

        return context;
    }

    private TestResultsController GetController(ApplicationDbContext context)
    {
        var controller = new TestResultsController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user1")
        }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
        };

        return controller;
    }

    [Fact]
    public void GetNotCheckedTestsCount_Returns_CorrectCount()
    {
        var context = GetInMemoryContext();
        var controller = new TestResultsController(context);

        var result = controller.GetNotCheckedTestsCount() as JsonResult;


        var property = result.Value.GetType().GetProperty("notCheckedCount");
        Assert.NotNull(property);

        var value = property.GetValue(result.Value);

        Assert.Equal(1, value);

    }

    [Fact]
    public async Task Index_ReturnsViewWithTestResults()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.Generic.List<TestResult>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_IfIdNull()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_IfNotFound()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsView_IfFound()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TestResult>(viewResult.Model);
        Assert.Equal(1, model.TestResultId);
    }

    [Fact]
    public void Create_Get_ReturnsViewWithSelectLists()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = controller.Create() as ViewResult;
        Assert.NotNull(result);

        Assert.NotNull(result.ViewData["JudgeId"]);
        Assert.NotNull(result.ViewData["TestId"]);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var newTestResult = new TestResult
        {
            JudgeId = 1,
            TestId = 1,
            Score = 90,
            DateTaken = DateTime.UtcNow,
            Explanation = "Good"
        };

        var result = await controller.Create(newTestResult);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        Assert.True(context.TestResults.Any(tr => tr.Explanation == "Good"));
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_IfIdNull()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_IfTestResultNotFound()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsView_IfFound()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TestResult>(viewResult.Model);
        Assert.Equal(1, model.TestResultId);
    }

    [Fact]
    public async Task Edit_Post_ReturnsNotFound_IfIdMismatch()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var testResult = context.TestResults.First();

        var result = await controller.Edit(testResult.TestResultId + 1, testResult);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var testResult = context.TestResults.First();
        testResult.Score = 95;
        testResult.DateTaken = DateTime.UtcNow;

        var result = await controller.Edit(testResult.TestResultId, testResult);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var updated = context.TestResults.Find(testResult.TestResultId);
        Assert.Equal(95, updated.Score);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFound_IfIdNull()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFound_IfTestResultNotFound()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsView_IfFound()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.Delete(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TestResult>(viewResult.Model);
        Assert.Equal(1, model.TestResultId);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesTestResult_AndRedirects()
    {
        var context = GetInMemoryContext();
        var controller = GetController(context);

        var result = await controller.DeleteConfirmed(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        Assert.False(context.TestResults.Any(tr => tr.TestResultId == 1));
    }
}
