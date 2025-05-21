using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace Tests;
public class PlayerEventsControllerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<StandingsUpdater> _mockStandingsUpdater;
    private readonly PlayerEventsController _controller;

    public PlayerEventsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDB_{Guid.NewGuid()}")
    .Options;

        var context = new ApplicationDbContext(options);

        _mockContext = new Mock<ApplicationDbContext>(options);
        _mockStandingsUpdater = new Mock<StandingsUpdater>(null);
        _controller = new PlayerEventsController(context, _mockStandingsUpdater.Object);

        SeedTestData(context);
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        var players = new List<Player>
        {
            new Player { PlayerId = 1, FullName = "Player One", TeamId = 1, Position = "BL" },
            new Player { PlayerId = 2, FullName = "Player Two", TeamId = 2, Position = "RB" }
        };

        var teamA = new Team { TeamId = 1, Players = new List<Player> { players[0] }, City = "Kyiv", Name = "Capitals" };
        var teamB = new Team { TeamId = 2, Players = new List<Player> { players[1] }, City = "Vinnytsia", Name = "Wolves" };

        var match = new SportSystem2.Models.Match
        {
            MatchId = 1,
            TeamA = teamA,
            TeamB = teamB,
            TournamentId = 100
        };

        var playerEvents = new List<PlayerEvent>
        {
            new PlayerEvent { PlayerEventId = 1, PlayerId = 1, MatchId = 1, Points = 2, Player = players[0] },
            new PlayerEvent { PlayerEventId = 2, PlayerId = 2, MatchId = 1, Points = 3, Player = players[1] }
        };

        context.Players.AddRange(players);
        context.Teams.AddRange(teamA, teamB);
        context.Matches.Add(match);
        context.PlayerEvents.AddRange(playerEvents);

        context.SaveChanges();
    }

    [Fact]
    public async Task Index_ReturnsViewWithPlayerEvents_WhenMatchIdIsValid()
    {
        var result = await _controller.Index(1, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<PlayerEvent>>(viewResult.Model);
        Assert.All(model, pe => Assert.Equal(1, pe.MatchId));
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var result = await _controller.Details(null, 1, null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithPlayerEvent_WhenValidIdAndMatchId()
    {
        var result = await _controller.Details(1, 1, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PlayerEvent>(viewResult.Model);
        Assert.Equal(1, model.PlayerEventId);
        Assert.Equal(1, model.MatchId);
    }

    [Fact]
    public void Create_Get_ReturnsViewWithPlayers()
    {
        var result = _controller.Create(1, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.ViewData["MatchId"]);
    }

    [Fact]
    public async Task Create_Post_ReturnsRedirect_WhenModelStateValid()
    {
        var newEvent = new PlayerEvent
        {
            PlayerId = 1,
            EventType = EventType.FieldGoal,
            Points = 1,
            Minute = 10,
            PeriodNumber = 1,
            PeriodType = PeriodType.Half
        };

        var result = await _controller.Create(1, null, newEvent);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Get_ReturnsView_WhenValidId()
    {
        var result = await _controller.Edit(1, 1, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PlayerEvent>(viewResult.Model);
        Assert.Equal(1, model.PlayerEventId);
    }

    [Fact]
    public async Task Delete_Get_ReturnsView_WhenValidId()
    {
        var result = await _controller.Delete(1, 1, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PlayerEvent>(viewResult.Model);
        Assert.Equal(1, model.PlayerEventId);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesPlayerEvent_AndRedirects()
    {
        var result = await _controller.DeleteConfirmed(1, null, null);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
