using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using Xunit;
namespace Tests;
public class MatchAnalysisControllerTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private Mock<UserManager<ApplicationUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        return mgr;
    }

    [Fact]
    public async Task Index_ReturnsAllMatchAnalyses()
    {
        var context = GetInMemoryDbContext();
        var judge = new Judge { JudgeId = 1, FullName = "Judge1", QualificationLevel = "Unqualified", ApplicationUserId = Guid.NewGuid().ToString() };
        var match = new SportSystem2.Models.Match { MatchId = 1, TeamA = new Team { Name = "A", City = "Kyiv" }, TeamB = new Team { Name = "B", City = "Kyiv" }, Tournament = new Tournament { Name = "Tour" }, TournamentRound = new TournamentRound { RoundName = "Round1", Location = "Kyiv" } };
        
        context.Matches.Add(match);
        context.Judges.Add(judge);
        var analysis = new MatchAnalysis
        {
            MatchAnalysisId = 1,
            MatchId = 1,
            CreatedByJudgeId = 1,
            Title = "Analysis 1",
            CreatedByJudge = judge,
            Match = match,
            Content = "1234567890"
        };
        context.MatchAnalyses.Add(analysis);
        await context.SaveChangesAsync();

        var userManager = GetMockUserManager().Object;

        var controller = new MatchAnalysisController(context, userManager);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<MatchAnalysis>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal("Analysis 1", model.First().Title);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_IfIdIsNull()
    {
        var context = GetInMemoryDbContext();
        var userManager = GetMockUserManager().Object;
        var controller = new MatchAnalysisController(context, userManager);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_IfAnalysisNotFound()
    {
        var context = GetInMemoryDbContext();
        var userManager = GetMockUserManager().Object;
        var controller = new MatchAnalysisController(context, userManager);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithModel()
    {
        var context = GetInMemoryDbContext();
        var judge = new Judge { JudgeId = 1, FullName = "Judge1", QualificationLevel = "Unqualified", ApplicationUserId = Guid.NewGuid().ToString() };
        var match = new SportSystem2.Models.Match { MatchId = 1, TeamA = new Team { Name = "A", City = "Kyiv" }, TeamB = new Team { Name = "B", City = "Kyiv" }, Tournament = new Tournament { Name = "Tour" }, TournamentRound = new TournamentRound { RoundName = "Round1", Location = "Kyiv" } };
        var analysis = new MatchAnalysis
        {
            MatchAnalysisId = 1,
            MatchId = 1,
            CreatedByJudgeId = 1,
            Title = "Analysis 1",
            CreatedByJudge = judge,
            Match = match,
            Content = "1234567890"
        };

        context.Judges.Add(judge);
        context.Matches.Add(match);
        context.MatchAnalyses.Add(analysis);
        await context.SaveChangesAsync();

        var userManager = GetMockUserManager().Object;
        var controller = new MatchAnalysisController(context, userManager);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MatchAnalysis>(viewResult.Model);
        Assert.Equal("Analysis 1", model.Title);
    }

    [Fact]
    public async Task Create_GET_ReturnsForbidIfJudgeNotFound()
    {
        var context = GetInMemoryDbContext();
        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var controller = new MatchAnalysisController(context, mockUserManager.Object);

        var result = await controller.Create(null);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Create_GET_ReturnsViewIfJudgeFound()
    {
        var context = GetInMemoryDbContext();
        var judge = new Judge { JudgeId = 1, ApplicationUserId = "user1", FullName = "Judge1", QualificationLevel = "Unqualified" };
        context.Judges.Add(judge);
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");

        var controller = new MatchAnalysisController(context, mockUserManager.Object);

        var result = await controller.Create(5);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.ViewData["MatchesDescriptions"]);
        Assert.Equal(5, viewResult.ViewData["MatchId"]);
        Assert.Equal(judge.JudgeId, viewResult.ViewData["JudgeId"]);
    }

    [Fact]
    public async Task Create_POST_AddsMatchAnalysisAndRedirects()
    {
        var context = GetInMemoryDbContext();
        var judge = new Judge { JudgeId = 1, ApplicationUserId = "user1", FullName = "Judge1", QualificationLevel = "Unqualified" };
        var match = new SportSystem2.Models.Match { MatchId = 1 };
        context.Judges.Add(judge);
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var mockUserManager = GetMockUserManager();
        var controller = new MatchAnalysisController(context, mockUserManager.Object);

        var newAnalysis = new MatchAnalysis
        {
            MatchId = 1,
            Title = "New Analysis",
            Content = "Some content",
            CreatedByJudgeId = 1,
            MinuteFrom = new TimeSpan(2010, 1, 1, 8, 0, 15),
            MinuteTo = new TimeSpan(2010, 1, 1, 8, 0, 15)
        };

        var result = await controller.Create(newAnalysis, 1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(MatchAnalysisController.Index), redirectResult.ActionName);
        Assert.Single(context.MatchAnalyses);
        Assert.Equal("New Analysis", context.MatchAnalyses.First().Title);
    }

    [Fact]
    public async Task Edit_GET_ReturnsNotFoundIfIdNullOrNotFound()
    {
        var context = GetInMemoryDbContext();
        var userManager = GetMockUserManager().Object;
        var controller = new MatchAnalysisController(context, userManager);

        var resultNull = await controller.Edit(null);
        Assert.IsType<NotFoundResult>(resultNull);

        var resultNotFound = await controller.Edit(123);
        Assert.IsType<NotFoundResult>(resultNotFound);
    }

    [Fact]
    public async Task Edit_GET_ReturnsViewWithModel()
    {
        var context = GetInMemoryDbContext();
        var judge = new Judge { JudgeId = 1, FullName = "Judge1", QualificationLevel = "Unqualified", ApplicationUserId = Guid.NewGuid().ToString() };
        var match = new SportSystem2.Models.Match { MatchId = 1, TeamA = new Team { Name = "A", City = "Kyiv" }, TeamB = new Team { Name = "B", City = "Kyiv" }, Tournament = new Tournament { Name = "Tour" }, TournamentRound = new TournamentRound { RoundName = "Round1", Location = "Kyiv" } };
        var analysis = new MatchAnalysis
        {
            MatchAnalysisId = 1,
            MatchId = 1,
            CreatedByJudgeId = 1,
            Title = "Analysis 1",
            CreatedByJudge = judge,
            Match = match,
            Content = "1234567890"
        };

        context.Judges.Add(judge);
        context.Matches.Add(match);
        context.MatchAnalyses.Add(analysis);
        await context.SaveChangesAsync();

        var userManager = GetMockUserManager().Object;
        var controller = new MatchAnalysisController(context, userManager);

        var result = await controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MatchAnalysis>(viewResult.Model);
        Assert.Equal("Analysis 1", model.Title);
    }
}
