using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;
namespace Tests;
public class TournamentRoundsControllerTests : IAsyncLifetime
{
    private ApplicationDbContext _context;
    private TournamentRoundsController _controller;

    public async Task InitializeAsync()
    {
        _context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        _controller = new TournamentRoundsController(_context, new StandingsUpdater(_context));

        _context.Tournaments.Add(new Tournament { TournamentId = 1, Name = "Default Tournament", Type = TournamentType.GroupStage });
        await _context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _context.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithRounds()
    {
        _context.TournamentRounds.Add(new TournamentRound { RoundId = 1, TournamentId = 1, RoundName = "Round 1", Location = "Vinnytsia" });
        await _context.SaveChangesAsync();

        var result = await _controller.Index(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<TournamentRound>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_IfIdIsNull()
    {
        var result = await _controller.Details(null, null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        var round = new TournamentRound
        {
            RoundId = 2,
            TournamentId = 1,
            RoundName = "Round A",
            Location = "City Stadium",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        var result = await _controller.Create(round);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Edit_Post_UpdatesCorrectly()
    {
        _context.TournamentRounds.Add(new TournamentRound { RoundId = 3, TournamentId = 1, RoundName = "Original", Location = "Vinnytsia" });
        await _context.SaveChangesAsync();

        var tracked = await _context.TournamentRounds.FindAsync(3);
        _context.Entry(tracked).State = EntityState.Detached;

        var updatedRound = new TournamentRound
        {
            RoundId = 3,
            TournamentId = 1,
            RoundName = "Updated",
            Location = "Field A",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        var result = await _controller.Edit(3, updatedRound, 1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var dbRound = await _context.TournamentRounds.FindAsync(3);
        Assert.Equal("Updated", dbRound.RoundName);
    }

    [Fact]
    public async Task DeleteConfirmed_DeletesRoundAndMatches()
    {
        var round = new TournamentRound { RoundId = 4, TournamentId = 1, RoundName = "Delete Me", Location = "Kyiv" };
        var match = new Match { MatchId = 1, TournamentRoundId = 4 };
        var assignment = new GameAssignment { GameAssignmentId = 1, MatchId = 1 };

        round.Matches = new List<Match> { match };
        match.GameAssignments = new List<GameAssignment> { assignment };

        _context.TournamentRounds.Add(round);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteConfirmed(4, 1);

        Assert.Null(await _context.TournamentRounds.FindAsync(4));
        Assert.Empty(_context.Matches);
        Assert.Empty(_context.GameAssignments);
    }
}
