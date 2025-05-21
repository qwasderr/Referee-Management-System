using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Models.DTO;
namespace SportSystem2.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TournamentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Tournaments.ToListAsync());
        }

        /*public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournament
                .FirstOrDefaultAsync(m => m.TournamentId == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }*/

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(m => m.TournamentId == id);

            if (tournament == null) return NotFound();

            var standings = await _context.TeamStandings
                .Where(ts => ts.TournamentId == id)
                .Include(ts => ts.Team)
                .OrderByDescending(ts => ts.Points)
                .ThenByDescending(ts => ts.Wins)
                .ToListAsync();

            var teamStats = standings.Select((ts, index) => new TeamStandingDTO
            {
                TeamName = ts.Team?.Name,
                TeamPhotoUrl = ts.Team?.PhotoPath,
                Wins = ts.Wins,
                Draws = ts.Draws,
                Losses = ts.Losses,
                Points = ts.Points,
                Scored = ts.Scored,
                Conceded = ts.Conceded,
                Position = index + 1
            }).ToList();

            ViewData["Standings"] = teamStats;

            return View(tournament);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TournamentId,Name,Type")] Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tournament);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TournamentId,Name,Type")] Tournament tournament)
        {
            if (id != tournament.TournamentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.TournamentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(m => m.TournamentId == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        /*[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                _context.Tournaments.Remove(tournament);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Rounds)
                    .ThenInclude(r => r.Matches)
                        .ThenInclude(m => m.GameAssignments)
                .FirstOrDefaultAsync(t => t.TournamentId == id);

            if (tournament == null)
            {
                return NotFound();
            }

            foreach (var round in tournament.Rounds)
            {
                foreach (var match in round.Matches)
                {
                    _context.GameAssignments.RemoveRange(match.GameAssignments);
                }

                _context.Matches.RemoveRange(round.Matches);
                var analysesToRemove = _context.MatchAnalyses.Where(ma => round.Matches.Select(m => m.MatchId).Contains(ma.MatchId));

                _context.MatchAnalyses.RemoveRange(analysesToRemove);
            }

            _context.TournamentRounds.RemoveRange(tournament.Rounds);

            _context.Tournaments.Remove(tournament);
            var standings = await _context.TeamStandings
                .Where(ts => ts.TournamentId == id)
                .ToListAsync();
            _context.TeamStandings.RemoveRange(standings);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool TournamentExists(int id)
        {
            return _context.Tournaments.Any(e => e.TournamentId == id);
        }
    }
}
