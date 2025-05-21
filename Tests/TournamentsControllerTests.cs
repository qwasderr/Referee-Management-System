using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
namespace Tests;
public class TournamentsControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "SportTestDb")
            .Options;

        var context = new ApplicationDbContext(options);

        if (!context.Tournaments.Any())
        {
            context.Tournaments.Add(new Tournament { TournamentId = 1, Name = "Test Tournament", Type = TournamentType.GroupStage });
            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfTournaments()
    {
        var context = GetDbContext();
        var controller = new TournamentsController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Tournament>>(viewResult.Model);
        Assert.NotEmpty(model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var context = GetDbContext();
        var controller = new TournamentsController(context);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenTournamentNotFound()
    {
        var context = GetDbContext();
        var controller = new TournamentsController(context);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewResult_WithTournament()
    {
        var context = GetDbContext();
        var controller = new TournamentsController(context);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Tournament>(viewResult.Model);
        Assert.Equal(1, model.TournamentId);
    }

    [Fact]
    public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
    {
        var context = GetDbContext();
        var controller = new TournamentsController(context);

        var newTournament = new Tournament { TournamentId = 2, Name = "New Tournament", Type = TournamentType.Knockout };

        var result = await controller.Create(newTournament);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(controller.Index), redirectResult.ActionName);

        Assert.True(context.Tournaments.Any(t => t.TournamentId == 2));
    }

    [Fact]
    public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = new TournamentsController(context);

        var tournament = new Tournament { TournamentId = 2, Name = "Tournament", Type = TournamentType.Knockout };

        var result = await controller.Edit(1, tournament);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesTournament_AndRedirectsToIndex()
    {
        var context = GetDbContext();

        var tournament = new Tournament { TournamentId = 3, Name = "ToDelete", Type = TournamentType.GroupStage };
        context.Tournaments.Add(tournament);
        context.SaveChanges();

        var controller = new TournamentsController(context);

        var result = await controller.DeleteConfirmed(3);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(controller.Index), redirectResult.ActionName);

        Assert.False(context.Tournaments.Any(t => t.TournamentId == 3));
    }
}
