using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace Tests;
public class MatchesControllerTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private Mock<IStandingsUpdater> GetMockStandingsUpdater()
    {
        var mock = new Mock<IStandingsUpdater>();
        mock.Setup(su => su.UpdateTeamStandingsAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetInMemoryDbContext();
        var standingsUpdaterMock = GetMockStandingsUpdater();
        var controller = new MatchesController(context, standingsUpdaterMock.Object);

        var result = await controller.Details(null, null, null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_AddsMatch()
    {
        var context = GetInMemoryDbContext();
        var standingsUpdaterMock = GetMockStandingsUpdater();
        var controller = new MatchesController(context, standingsUpdaterMock.Object);

        var tournamentRound = new TournamentRound
        {
            RoundId = 1,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(1),
            TournamentId = 1,
            Location = "Kyiv",
            RoundName = "Round 1"
        };
        context.TournamentRounds.Add(tournamentRound);
        await context.SaveChangesAsync();

        var match = new SportSystem2.Models.Match
        {
            TeamAId = 1,
            TeamBId = 2,
            TournamentRoundId = 1,
            TournamentId = 1,
            Date = DateTime.Today
        };

        var result = await controller.Create(match, 1, 1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        Assert.Single(context.Matches);
        Assert.Equal(match.TeamAId, context.Matches.First().TeamAId);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesMatchAndResults()
    {
        var context = GetInMemoryDbContext();
        var standingsUpdaterMock = GetMockStandingsUpdater();
        var controller = new MatchesController(context, standingsUpdaterMock.Object);

        var match = new SportSystem2.Models.Match
        {
            MatchId = 1,
            TeamAId = 1,
            TeamBId = 2,
            TournamentId = 1,
            TournamentRoundId = 1,
            Date = DateTime.Today
        };
        context.Matches.Add(match);
        context.MatchResults.AddRange(
            new MatchResult { MatchId = 1, TeamId = 1, Points = 0 },
            new MatchResult { MatchId = 1, TeamId = 2, Points = 0 }
        );
        context.TournamentRounds.Add(new TournamentRound { RoundId = 1, StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(1), RoundName = "Round 1", Location = "Lviv" });
        await context.SaveChangesAsync();

        match.Date = DateTime.Today;
        var result = await controller.Edit(1, match, ScoreA: 3, ScoreB: 1, TournamentId: 1, TournamentRoundId: 1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var updatedResultA = context.MatchResults.First(r => r.TeamId == 1 && r.MatchId == 1);
        var updatedResultB = context.MatchResults.First(r => r.TeamId == 2 && r.MatchId == 1);
        Assert.Equal(3, updatedResultA.Points);
        Assert.Equal(1, updatedResultB.Points);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesMatchAndRelatedEntities()
    {
        var context = GetInMemoryDbContext();
        var standingsUpdaterMock = GetMockStandingsUpdater();
        var controller = new MatchesController(context, standingsUpdaterMock.Object);

        var match = new SportSystem2.Models.Match { MatchId = 1, TournamentId = 1 };
        context.Matches.Add(match);
        context.MatchResults.Add(new MatchResult { MatchId = 1, TeamId = 1, Points = 0 });
        context.PlayerEvents.Add(new PlayerEvent { PlayerEventId = 1, MatchId = 1 });
        context.GameAssignments.Add(new GameAssignment { GameAssignmentId = 1, MatchId = 1 });
        context.MatchAnalyses.Add(new MatchAnalysis { MatchAnalysisId = 1, MatchId = 1, Content = "1234567", Title = "Analysis1" });
        await context.SaveChangesAsync();

        var result = await controller.DeleteConfirmed(1, 1, null);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        Assert.Empty(context.Matches);
        Assert.Empty(context.MatchResults);
        Assert.Empty(context.PlayerEvents);
        Assert.Empty(context.GameAssignments);
        Assert.Empty(context.MatchAnalyses);
    }
}
