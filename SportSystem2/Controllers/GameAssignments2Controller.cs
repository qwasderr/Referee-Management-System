using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;

namespace SportSystem2.Controllers
{
    public class GameAssignments2Controller : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameAssignments2Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? matchId, int? tournamentRoundId)
        {
            if (matchId == null)
            {
                return BadRequest("MatchId is required.");
            }

            var query = _context.GameAssignments
                .Where(ga => ga.MatchId == matchId)
                .Include(g => g.Judge)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(g => g.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TournamentRound)
                .AsQueryable();

            if (tournamentRoundId != null)
            {
                query = query.Where(ga => ga.Match.TournamentRoundId == tournamentRoundId);
            }

            var gameAssignments = await query.ToListAsync();

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            ViewData["MatchesDescriptions"] = gameAssignments
                .GroupBy(ga => ga.MatchId)
                .ToDictionary(
                    g => g.Key,
                    g => $"{g.First().Match.TeamA.Name} - {g.First().Match.TeamB.Name} | {g.First().Match.Tournament.Name} | {g.First().Match.TournamentRound.RoundName}"
                );

            if (match != null)
            {
                ViewData["MatchDescription"] = $"{match.TeamA.Name} - {match.TeamB.Name} | {match.Tournament.Name} | {match.TournamentRound.RoundName}";
            }

            return View(gameAssignments);
        }

        public async Task<IActionResult> Details(int? id, int? matchId, int? tournamentRoundId)
        {
            if (id == null || matchId == null || tournamentRoundId == null)
            {
                return BadRequest("Required parameters are missing.");
            }

            var gameAssignment = await _context.GameAssignments
                .Include(g => g.Judge)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(g => g.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.GameAssignmentId == id);

            if (gameAssignment == null)
            {
                return NotFound();
            }

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;
            ViewData["MatchDescription"] = $"{gameAssignment.Match.TeamA.Name} - {gameAssignment.Match.TeamB.Name} | {gameAssignment.Match.Tournament.Name} | {gameAssignment.Match.TournamentRound.RoundName}";

            return View(gameAssignment);
        }

        public IActionResult Create(int? matchId, int? tournamentRoundId)
        {
            if (matchId == null)
            {
                return BadRequest("MatchId is required.");
            }

            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName");

            var match = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .FirstOrDefault(m => m.MatchId == matchId);

            if (match == null)
                return NotFound();

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;
            ViewData["MatchDescription"] = $"{match.TeamA.Name} - {match.TeamB.Name} | {match.Tournament.Name} | {match.TournamentRound.RoundName}";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("GameAssignmentId,JudgeId,MatchId,Role")] GameAssignment gameAssignment, int? matchId, int? tournamentRoundId)
        {
            if (matchId == null || gameAssignment.MatchId != matchId)
            {
                return BadRequest("MatchId mismatch.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(gameAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { matchId = matchId, tournamentRoundId = tournamentRoundId });
            }

            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", gameAssignment.JudgeId);
            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(gameAssignment);
        }

        public async Task<IActionResult> Edit(int? id, int? matchId, int? tournamentRoundId)
        {
            if (id == null || matchId == null)
            {
                return BadRequest();
            }

            var gameAssignment = await _context.GameAssignments.FindAsync(id);
            if (gameAssignment == null)
            {
                return NotFound();
            }

            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", gameAssignment.JudgeId);
            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(gameAssignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("GameAssignmentId,JudgeId,MatchId,Role")] GameAssignment gameAssignment, int? matchId, int? tournamentRoundId)
        {
            if (id != gameAssignment.GameAssignmentId || matchId == null || gameAssignment.MatchId != matchId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.GameAssignments.Any(e => e.GameAssignmentId == gameAssignment.GameAssignmentId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { matchId = matchId, tournamentRoundId = tournamentRoundId });
            }

            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", gameAssignment.JudgeId);
            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(gameAssignment);
        }

        public async Task<IActionResult> Delete(int? id, int? matchId, int? tournamentRoundId)
        {
            if (id == null || matchId == null)
            {
                return BadRequest();
            }

            var gameAssignment = await _context.GameAssignments
                .Include(g => g.Judge)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(g => g.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(g => g.Match)
                    .ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.GameAssignmentId == id);

            if (gameAssignment == null)
            {
                return NotFound();
            }

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;
            ViewData["MatchDescription"] = $"{gameAssignment.Match.TeamA.Name} - {gameAssignment.Match.TeamB.Name} | {gameAssignment.Match.Tournament.Name} | {gameAssignment.Match.TournamentRound.RoundName}";

            return View(gameAssignment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, int? matchId, int? tournamentRoundId)
        {
            var gameAssignment = await _context.GameAssignments.FindAsync(id);
            if (gameAssignment != null)
            {
                _context.GameAssignments.Remove(gameAssignment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { matchId = matchId, tournamentRoundId = tournamentRoundId });
        }

        private bool GameAssignmentExists(int id)
        {
            return _context.GameAssignments.Any(e => e.GameAssignmentId == id);
        }
    }
}
