using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class GameAssignmentsControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private ClaimsPrincipal GetFakeJudgePrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Judge")
            }, "mock"));
        }

        private void SetUser(Controller controller, ClaimsPrincipal user)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewWithAssignments_WhenJudgeIsAuthenticated()
        {
            var context = GetDbContext();

            var judge = new Judge {JudgeId = 1, ApplicationUserId = "judge1", FullName = "Name", QualificationLevel = "Unqualified" };
            var teamA = new Team { TeamId = 1, Name = "Team A", City = "Vinnytsia" };
            var teamB = new Team { TeamId = 2, Name = "Team B", City = "Vinnytsia" };
            var tournament = new Tournament { TournamentId = 1, Name = "Championship" };
            var round = new TournamentRound { RoundName = "Quarterfinal", Location = "Stadium" };
            var match = new SportSystem2.Models.Match
            {
                MatchId = 1,
                TeamA = teamA,
                TeamB = teamB,
                Tournament = tournament,
                TournamentRound = round,
                Date = DateTime.Now
            };
            var assignment = new GameAssignment
            {
                GameAssignmentId = 1,
                JudgeId = judge.JudgeId,
                Match = match,
                MatchId = match.MatchId,
                Role = JudgeRole.R
            };

            await context.Judges.AddAsync(judge);
            await context.Teams.AddRangeAsync(teamA, teamB);
            await context.Tournaments.AddAsync(tournament);
            await context.TournamentRounds.AddAsync(round);
            await context.Matches.AddAsync(match);
            await context.GameAssignments.AddAsync(assignment);
            await context.SaveChangesAsync();

            var controller = new GameAssignmentsController(context);
            SetUser(controller, GetFakeJudgePrincipal("judge1"));

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<GameAssignment>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenGameAssignmentDoesNotExist()
        {
            var context = GetDbContext();
            await context.Judges.AddAsync(new Judge { JudgeId = 1, ApplicationUserId = "judge1", FullName = "Name", QualificationLevel = "Unqualified" });
            await context.SaveChangesAsync();

            var controller = new GameAssignmentsController(context);
            SetUser(controller, GetFakeJudgePrincipal("judge1"));

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewResult()
        {
            var context = GetDbContext();
            await context.Judges.AddAsync(new Judge { JudgeId = 1, ApplicationUserId = "judge1", FullName = "Name", QualificationLevel = "Unqualified" });
            await context.SaveChangesAsync();

            var controller = new GameAssignmentsController(context);
            SetUser(controller, GetFakeJudgePrincipal("judge1"));

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_Post_UpdatesAssignment_WhenValid()
        {
            var context = GetDbContext();

            var judge = new Judge {JudgeId = 1, ApplicationUserId = "judge1", FullName = "Name", QualificationLevel = "Unqualified" };
            var match = new SportSystem2.Models.Match { MatchId = 1 };
            var assignment = new GameAssignment
            {
                GameAssignmentId = 1,
                JudgeId = 1,
                MatchId = 1,
                Role = JudgeRole.SJ
            };

            await context.Judges.AddAsync(judge);
            await context.Matches.AddAsync(match);
            await context.GameAssignments.AddAsync(assignment);
            await context.SaveChangesAsync();

            var controller = new GameAssignmentsController(context);
            SetUser(controller, GetFakeJudgePrincipal("judge1"));

            var updatedAssignment = new GameAssignment
            {
                GameAssignmentId = 1,
                MatchId = 1,
                Role = JudgeRole.FJ
            };

            var result = await controller.Edit(1, updatedAssignment);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var modifiedAssignment = await context.GameAssignments.FindAsync(1);
            Assert.Equal(JudgeRole.FJ, modifiedAssignment.Role);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesAssignment()
        {
            var context = GetDbContext();
            var judge = new Judge { JudgeId = 1, ApplicationUserId = "judge1", FullName = "Name", QualificationLevel = "Unqualified" };
            var assignment = new GameAssignment
            {
                GameAssignmentId = 1,
                JudgeId = 1,
                MatchId = 1,
                Role = JudgeRole.R
            };

            await context.Judges.AddAsync(judge);
            await context.GameAssignments.AddAsync(assignment);
            await context.SaveChangesAsync();

            var controller = new GameAssignmentsController(context);
            SetUser(controller, GetFakeJudgePrincipal("judge1"));

            var result = await controller.DeleteConfirmed(1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(context.GameAssignments);
        }
    }
}
