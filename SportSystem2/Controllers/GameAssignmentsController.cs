using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using System.Security.Claims;

namespace SportSystem2.Controllers
{
    [Authorize(Roles = "Judge")]
    public class GameAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameAssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Judge?> GetCurrentJudgeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == userId);
        }

        public async Task<IActionResult> Index()
        {
            var judge = await GetCurrentJudgeAsync();
            if (judge == null)
                return Unauthorized();

            var gameAssignments = await _context.GameAssignments
                .Where(ga => ga.JudgeId == judge.JudgeId)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(g => g.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TournamentRound)
                .ToListAsync();

            ViewData["MatchesDescriptions"] = gameAssignments
                    .GroupBy(ga => ga.MatchId)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var m = g.First().Match;
                            return $"{m.TeamA.Name} - {m.TeamB.Name} | {m.Tournament.Name} | {m.TournamentRound.RoundName} | {m.TournamentRound.Location} | {m.Date:dd.MM.yyyy HH:mm}";
                        });

            return View(gameAssignments);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            var gameAssignment = await _context.GameAssignments
                .Include(g => g.Match).ThenInclude(m => m.TeamA)
                .Include(g => g.Match).ThenInclude(m => m.TeamB)
                .Include(g => g.Match).ThenInclude(m => m.Tournament)
                .Include(g => g.Match).ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.GameAssignmentId == id && m.JudgeId == judge.JudgeId);

            if (gameAssignment == null) return NotFound();

            ViewData["MatchDescription"] = $"{gameAssignment.Match.TeamA.Name} - {gameAssignment.Match.TeamB.Name} | {gameAssignment.Match.Tournament.Name} | {gameAssignment.Match.TournamentRound.RoundName} | {gameAssignment.Match.TournamentRound.Location}  |  {gameAssignment.Match.Date:dd.MM.yyyy HH:mm}";

            return View(gameAssignment);
        }

        public async Task<IActionResult> Create()
        {
            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .Select(m => new
                {
                    m.MatchId,
                    Description = $"{m.TeamA.Name} - {m.TeamB.Name} | {m.Tournament.Name} | {m.TournamentRound.RoundName}"
                }).ToListAsync();

            ViewData["MatchId"] = new SelectList(matches, "MatchId", "Description");

            return View(new GameAssignment { JudgeId = judge.JudgeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("MatchId,Role")] GameAssignment gameAssignment)
        {
            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            gameAssignment.JudgeId = judge.JudgeId;

            if (ModelState.IsValid)
            {
                _context.Add(gameAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .Select(m => new
                {
                    m.MatchId,
                    Description = $"{m.TeamA.Name} - {m.TeamB.Name} | {m.Tournament.Name} | {m.TournamentRound.RoundName}"
                }).ToListAsync();

            ViewData["MatchId"] = new SelectList(matches, "MatchId", "Description", gameAssignment.MatchId);

            return View(gameAssignment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            var gameAssignment = await _context.GameAssignments
                .FirstOrDefaultAsync(g => g.GameAssignmentId == id && g.JudgeId == judge.JudgeId);

            if (gameAssignment == null) return NotFound();

            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .Select(m => new
                {
                    m.MatchId,
                    Description = $"{m.TeamA.Name} - {m.TeamB.Name} | {m.Tournament.Name} | {m.TournamentRound.RoundName}"
                }).ToListAsync();

            ViewData["MatchId"] = new SelectList(matches, "MatchId", "Description", gameAssignment.MatchId);

            return View(gameAssignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("GameAssignmentId,MatchId,Role")] GameAssignment gameAssignment)
        {
            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            var existingAssignment = await _context.GameAssignments
                .FirstOrDefaultAsync(g => g.GameAssignmentId == id && g.JudgeId == judge.JudgeId);

            if (existingAssignment == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingAssignment.MatchId = gameAssignment.MatchId;
                    existingAssignment.Role = gameAssignment.Role;

                    _context.Update(existingAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameAssignmentExists(id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .Select(m => new
                {
                    m.MatchId,
                    Description = $"{m.TeamA.Name} - {m.TeamB.Name} | {m.Tournament.Name} | {m.TournamentRound.RoundName}"
                }).ToListAsync();

            ViewData["MatchId"] = new SelectList(matches, "MatchId", "Description", gameAssignment.MatchId);

            return View(gameAssignment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            var gameAssignment = await _context.GameAssignments
                .Include(g => g.Match).ThenInclude(m => m.TeamA)
                .Include(g => g.Match).ThenInclude(m => m.TeamB)
                .Include(g => g.Match).ThenInclude(m => m.Tournament)
                .Include(g => g.Match).ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.GameAssignmentId == id && m.JudgeId == judge.JudgeId);

            if (gameAssignment == null) return NotFound();

            ViewData["MatchDescription"] = $"{gameAssignment.Match.TeamA.Name} - {gameAssignment.Match.TeamB.Name} | {gameAssignment.Match.Tournament.Name} | {gameAssignment.Match.TournamentRound.RoundName}";

            return View(gameAssignment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var judge = await GetCurrentJudgeAsync();
            if (judge == null) return Unauthorized();

            var gameAssignment = await _context.GameAssignments
                .FirstOrDefaultAsync(g => g.GameAssignmentId == id && g.JudgeId == judge.JudgeId);

            if (gameAssignment != null)
            {
                _context.GameAssignments.Remove(gameAssignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GameAssignmentExists(int id)
        {
            return _context.GameAssignments.Any(e => e.GameAssignmentId == id);
        }
    }
}
