using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;

namespace SportSystem2.Controllers
{
    public class Matches2Controller : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStandingsUpdater _standingsUpdater;

        public Matches2Controller(ApplicationDbContext context, IStandingsUpdater standingsUpdater)
        {
            _context = context;
            _standingsUpdater = standingsUpdater;
        }

        public async Task<IActionResult> Index(int? tournamentRoundId)
        {
            var matchesQuery = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.TournamentRound)
                .Include(m => m.Tournament)
                .Include(m => m.MatchResults)
                .AsQueryable();

            if (tournamentRoundId.HasValue)
            {
                matchesQuery = matchesQuery.Where(m => m.TournamentRoundId == tournamentRoundId.Value);
            }

            var matches = await matchesQuery.ToListAsync();

            var tournamentRound = await _context.TournamentRounds
                .Include(tr => tr.Tournament)
                .FirstOrDefaultAsync(tr => tr.RoundId == tournamentRoundId);

            ViewData["TournamentId"] = tournamentRound?.TournamentId ?? 0;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(matches);
        }

        public async Task<IActionResult> Details(int? id, int? tournamentId, int? tournamentRoundId)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .Include(m => m.MatchResults)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            var playerEvents = await _context.PlayerEvents
                .Include(pe => pe.Player)
                .Where(pe => pe.MatchId == id)
                .ToListAsync();

            var gameAssignments = await _context.GameAssignments
                .Include(ga => ga.Judge)
                .Where(ga => ga.MatchId == id)
                .ToListAsync();

            ViewData["PlayerEvents"] = playerEvents;
            ViewData["GameAssignments"] = gameAssignments;

            ViewData["TournamentId"] = tournamentId;
            ViewData["TournamentRoundId"] = tournamentRoundId;
            ViewData["MatchResults"] = match.MatchResults;

            return View(match);
        }

        public IActionResult Create()
        {
            ViewData["TeamAId"] = new SelectList(_context.Teams, "TeamId", "Name");
            ViewData["TeamBId"] = new SelectList(_context.Teams, "TeamId", "Name");
            ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name");
            ViewData["TournamentRoundId"] = new SelectList(_context.TournamentRounds, "RoundId", "RoundName");
            ViewData["TournamentRoundIdLoc"] = new SelectList(_context.TournamentRounds, "RoundId", "Location");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("MatchId,Date,TeamAId,TeamBId,TournamentId,TournamentRoundId")] Match match)
        {
            if (match.TeamAId == match.TeamBId)
            {
                ModelState.AddModelError("", "A team cannot play against itself.");
            }

            var tournamentRound = await _context.TournamentRounds
                .FirstOrDefaultAsync(tr => tr.RoundId == match.TournamentRoundId);

            if (tournamentRound != null)
            {
                if (match.Date < tournamentRound.StartDate || match.Date > tournamentRound.EndDate)
                {
                    ModelState.AddModelError("", "Match date must be within the tournament round date range.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(match);
                await _context.SaveChangesAsync();

                var matchResults = new List<MatchResult>
                {
                    new MatchResult { MatchId = match.MatchId, TeamId = match.TeamAId, Points = 0 },
                    new MatchResult { MatchId = match.MatchId, TeamId = match.TeamBId, Points = 0 }
                };

                _context.MatchResults.AddRange(matchResults);
                await _context.SaveChangesAsync();
                await _standingsUpdater.UpdateTeamStandingsAsync(match.TournamentId);
                return RedirectToAction(nameof(Index));
            }

            ViewData["TeamAId"] = new SelectList(_context.Teams, "TeamId", "Name", match.TeamAId);
            ViewData["TeamBId"] = new SelectList(_context.Teams, "TeamId", "Name", match.TeamBId);
            ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name", match.TournamentId);
            ViewData["TournamentRoundId"] = new SelectList(_context.TournamentRounds, "RoundId", "RoundName", match.TournamentRoundId);
            ViewData["TournamentRoundIdLoc"] = new SelectList(_context.TournamentRounds, "RoundId", "Location", match.TournamentRoundId);

            return View(match);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.MatchResults)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            ViewData["TeamAId"] = new SelectList(_context.Teams, "TeamId", "Name", match.TeamAId);
            ViewData["TeamBId"] = new SelectList(_context.Teams, "TeamId", "Name", match.TeamBId);
            ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name", match.TournamentId);
            ViewData["TournamentRoundId"] = new SelectList(_context.TournamentRounds, "RoundId", "RoundName", match.TournamentRoundId);
            ViewData["TournamentRoundIdLoc"] = new SelectList(_context.TournamentRounds, "RoundId", "Location", match.TournamentRoundId);

            return View(match);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Match match, int ScoreA, int ScoreB)
        {
            if (match.TeamAId == match.TeamBId)
            {
                ModelState.AddModelError("", "A team cannot play against itself.");
            }

            var tournamentRound = await _context.TournamentRounds
                .FirstOrDefaultAsync(tr => tr.RoundId == match.TournamentRoundId);

            if (tournamentRound != null)
            {
                if (match.Date < tournamentRound.StartDate || match.Date > tournamentRound.EndDate)
                {
                    ModelState.AddModelError("", "Match date must be within the tournament round date range.");
                }
            }

            if (id != match.MatchId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();

                    var results = _context.MatchResults
                        .Where(r => r.MatchId == id)
                        .ToList();

                    var resultA = results.FirstOrDefault(r => r.TeamId == match.TeamAId);
                    var resultB = results.FirstOrDefault(r => r.TeamId == match.TeamBId);

                    if (resultA != null)
                        resultA.Points = ScoreA;
                    else
                        _context.MatchResults.Add(new MatchResult { MatchId = id, TeamId = match.TeamAId, Points = ScoreA });

                    if (resultB != null)
                        resultB.Points = ScoreB;
                    else
                        _context.MatchResults.Add(new MatchResult { MatchId = id, TeamId = match.TeamBId, Points = ScoreB });

                    await _context.SaveChangesAsync();
                    await _standingsUpdater.UpdateTeamStandingsAsync(match.TournamentId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Matches.Any(e => e.MatchId == match.MatchId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index), new { tournamentRoundId = match.TournamentRoundId });
            }

            ViewData["TeamAId"] = new SelectList(_context.Teams, "TeamId", "Name", match.TeamAId);
            ViewData["TeamBId"] = new SelectList(_context.Teams, "TeamId", "Name", match.TeamBId);

            return View(match);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            return View(match);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, int? tournamentId, int? tournamentRoundId)
        {
            var match = await _context.Matches
                .Include(m => m.MatchResults)
                .Include(m => m.PlayerEvents)
                .Include(m => m.GameAssignments)
                .Include(m => m.MatchAnalyses)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match != null)
            {
                _context.MatchResults.RemoveRange(match.MatchResults);
                _context.PlayerEvents.RemoveRange(match.PlayerEvents);
                _context.GameAssignments.RemoveRange(match.GameAssignments);
                _context.MatchAnalyses.RemoveRange(match.MatchAnalyses);

                _context.Matches.Remove(match);
                await _context.SaveChangesAsync();

                if (tournamentId.HasValue)
                {
                    await _standingsUpdater.UpdateTeamStandingsAsync(tournamentId.Value);
                }
            }

            return RedirectToAction(nameof(Index), new { tournamentRoundId });
        }
    }
}
