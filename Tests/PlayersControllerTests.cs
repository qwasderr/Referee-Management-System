using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using Xunit;
namespace Tests;
public class PlayersControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        var team = new Team { TeamId = 1, Name = "Team A", City = "Vinnytsia" };
        context.Teams.Add(team);
        context.Players.Add(new Player
        {
            PlayerId = 1,
            TeamId = team.TeamId,
            Team = team,
            FullName = "Player One",
            Number = 10,
            PhotoPath = null,
            Position = "RB"
        });
        context.SaveChanges();

        return context;
    }

    private Mock<IWebHostEnvironment> GetMockEnvironment()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(m => m.WebRootPath).Returns(Directory.GetCurrentDirectory());
        return mockEnv;
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfPlayers()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Player>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenPlayerDoesNotExist()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewResult_WithPlayer()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Player>(viewResult.Model);
        Assert.Equal(1, model.PlayerId);
    }

    [Fact]
    public void Create_Get_ReturnsViewResult_WithTeamId()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = controller.Create(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(1, viewResult.ViewData["TeamId"]);
    }

    [Fact]
    public async Task Create_Post_ValidPlayer_AddsPlayerAndRedirects()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var newPlayer = new Player
        {
            TeamId = 1,
            FullName = "New Player",
            Number = 7,
            Position = "QB"
        };

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(0);

        var result = await controller.Create(newPlayer, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Teams", redirect.ControllerName);
        Assert.Equal(1, redirect.RouteValues["id"]);

        Assert.True(context.Players.Any(p => p.FullName == "New Player"));
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_WhenPlayerDoesNotExist()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewResult_WithPlayer()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var result = await controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Player>(viewResult.Model);
        Assert.Equal(1, model.PlayerId);
    }

    [Fact]
    public async Task DeleteConfirmed_DeletesPlayer_WhenNoRelatedEvents()
    {
        var context = GetDbContext();
        var env = GetMockEnvironment().Object;
        var controller = new PlayersController(context, env);

        var playerCountBefore = context.Players.Count();

        var result = await controller.DeleteConfirmed(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Teams", redirect.ControllerName);

        var playerCountAfter = context.Players.Count();
        Assert.True(playerCountAfter < playerCountBefore);
    }

}
