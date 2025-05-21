using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;
namespace Tests;
public class TeamsControllerTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        return context;
    }

    private Mock<IWebHostEnvironment> GetMockEnvironment()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(m => m.WebRootPath).Returns(Directory.GetCurrentDirectory());
        return mockEnv;
    }

    private Mock<IImageService> GetMockImageService()
    {
        var mock = new Mock<IImageService>();
        mock.Setup(s => s.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("fakepath.jpg");
        return mock;
    }

    [Fact]
    public async Task Index_ReturnsViewWithTeams()
    {
        var context = GetInMemoryContext();
        context.Teams.Add(new Team { TeamId = 5, Name = "Team A", City = "Lviv" });
        context.Teams.Add(new Team { TeamId = 6, Name = "Team B", City = "Lviv" });
        await context.SaveChangesAsync();

        var controller = new TeamsController(context, GetMockEnvironment().Object, GetMockImageService().Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Team>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetInMemoryContext();
        var controller = new TeamsController(context, GetMockEnvironment().Object, GetMockImageService().Object);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenTeamDoesNotExist()
    {
        var context = GetInMemoryContext();
        var controller = new TeamsController(context, GetMockEnvironment().Object, GetMockImageService().Object);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithTeam()
    {
        var context = GetInMemoryContext();
        context.Teams.Add(new Team { TeamId = 2, Name = "Test Team", City = "Vinnytsia" });
        await context.SaveChangesAsync();

        var controller = new TeamsController(context, GetMockEnvironment().Object, GetMockImageService().Object);

        var result = await controller.Details(2);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Team>(viewResult.Model);
        Assert.Equal(2, model.TeamId);
        Assert.Equal("Test Team", model.Name);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesTeam_WhenNoRelatedMatches()
    {
        var context = GetInMemoryContext();
        var team = new Team { TeamId = 1, Name = "To Delete", City = "Kyiv" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var controller = new TeamsController(context, GetMockEnvironment().Object, GetMockImageService().Object);

        var result = await controller.DeleteConfirmed(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        Assert.Empty(context.Teams);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsView_WhenTeamHasRelatedMatches()
    {
        var context = GetInMemoryContext();
        var team = new Team { TeamId = 1, Name = "Team with Matches", City = "Kyiv" };
        context.Teams.Add(team);
        context.Matches.Add(new SportSystem2.Models.Match { MatchId = 1, TeamAId = 1, TeamBId = 2 });
        await context.SaveChangesAsync();

        var controller = new TeamsController(context, GetMockEnvironment().Object, GetMockImageService().Object);

        var result = await controller.DeleteConfirmed(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Single(context.Teams);
    }
}
