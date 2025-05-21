using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;

namespace SportSystem2.Controllers
{
    public class PlayerEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStandingsUpdater _standingsUpdater;
        public PlayerEventsController(ApplicationDbContext context, IStandingsUpdater standingsUpdater)
        {
            _context = context;
            _standingsUpdater = standingsUpdater;
        }

        public async Task<IActionResult> Index(int? matchId, int? tournamentRoundId)
        {
            if (matchId == null) return BadRequest();

            var playerEvents = _context.PlayerEvents
                .Include(p => p.Match)
                .Include(p => p.Player)
                .Where(pe => pe.MatchId == matchId);

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;
            return View(await playerEvents.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id, int? matchId, int? tournamentRoundId)
        {
            if (id == null || matchId == null)
            {
                return NotFound();
            }

            var playerEvent = await _context.PlayerEvents
                .Include(p => p.Match)
                .Include(p => p.Player)
                .FirstOrDefaultAsync(m => m.PlayerEventId == id && m.MatchId == matchId);

            if (playerEvent == null)
            {
                return NotFound();
            }

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(playerEvent);
        }

        public IActionResult Create(int? matchId, int? tournamentRoundId)
        {
            if (matchId == null) return BadRequest();

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            var match = _context.Matches
                .Where(m => m.MatchId == matchId)
                .Include(m => m.TeamA).ThenInclude(t => t.Players)
                .Include(m => m.TeamB).ThenInclude(t => t.Players)
                .FirstOrDefault();

            if (match == null) return NotFound();

            var players = match.TeamA.Players
                .Concat(match.TeamB.Players)
                .Distinct()
                .ToList();

            ViewBag.PlayerId = new SelectList(players, "PlayerId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? matchId, int? tournamentRoundId, [Bind("PlayerEventId,PlayerId,MatchId,EventType,Yards,Points,Minute,PeriodNumber,PeriodType,Notes")] PlayerEvent playerEvent)
        {
            if (matchId == null) return BadRequest();

            if (ModelState.IsValid)
            {
                playerEvent.MatchId = matchId.Value;
                playerEvent.Player = _context.Players.Find(playerEvent.PlayerId);
                _context.Add(playerEvent);

                if (playerEvent.Points != null)
                {
                    var matchRes = _context.MatchResults
                        .FirstOrDefault(m => m.MatchId == matchId && m.TeamId == playerEvent.Player.TeamId);

                    if (matchRes != null)
                    {
                        matchRes.Points += playerEvent.Points.Value;
                    }
                }

                await _context.SaveChangesAsync();
                if (tournamentRoundId.HasValue)
                {
                    var tournamentId = _context.Matches
                        .Where(m => m.MatchId == matchId)
                        .Select(m => m.TournamentId)
                        .FirstOrDefault();

                    if (tournamentId != 0)
                    {
                        await _standingsUpdater.UpdateTeamStandingsAsync(tournamentId);
                    }
                }
                return RedirectToAction(nameof(Index), new { matchId = matchId, tournamentRoundId = tournamentRoundId });
            }

            var match = _context.Matches
                 .Where(m => m.MatchId == matchId)
                 .Include(m => m.TeamA).ThenInclude(t => t.Players)
                 .Include(m => m.TeamB).ThenInclude(t => t.Players)
                 .FirstOrDefault();

            if (match == null) return NotFound();

            var players = match.TeamA.Players
                .Concat(match.TeamB.Players)
                .Distinct()
                .ToList();

            ViewBag.PlayerId = new SelectList(players, "PlayerId", "FullName");
            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(playerEvent);
        }

        public async Task<IActionResult> Edit(int? id, int? matchId, int? tournamentRoundId)
        {
            if (id == null || matchId == null)
            {
                return NotFound();
            }

            var playerEvent = await _context.PlayerEvents.FindAsync(id);
            if (playerEvent == null || playerEvent.MatchId != matchId)
            {
                return NotFound();
            }
            var match = _context.Matches
                            .Where(m => m.MatchId == matchId)
                            .Include(m => m.TeamA).ThenInclude(t => t.Players)
                            .Include(m => m.TeamB).ThenInclude(t => t.Players)
                            .FirstOrDefault();

            if (match == null) return NotFound();

            var players = match.TeamA.Players
                .Concat(match.TeamB.Players)
                .Distinct()
                .ToList();

            ViewBag.PlayerId = new SelectList(players, "PlayerId", "FullName");
            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(playerEvent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, int? matchId, int? tournamentRoundId, [Bind("PlayerEventId,PlayerId,MatchId,EventType,Yards,Points,Minute,PeriodNumber,PeriodType,Notes")] PlayerEvent playerEvent)
        {
            if (matchId == null || id != playerEvent.PlayerEventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    playerEvent.MatchId = matchId.Value;
                    playerEvent.Player = _context.Players.Find(playerEvent.PlayerId);
                    _context.Update(playerEvent);

                    if (playerEvent.Points != null)
                    {
                        var matchRes = _context.MatchResults
                            .FirstOrDefault(m => m.MatchId == matchId && m.TeamId == playerEvent.Player.TeamId);

                        if (matchRes != null)
                        {
                            matchRes.Points += playerEvent.Points.Value;
                        }
                    }

                    await _context.SaveChangesAsync();

                    if (tournamentRoundId.HasValue)
                    {
                        var tournamentId = _context.Matches
                            .Where(m => m.MatchId == matchId)
                            .Select(m => m.TournamentId)
                            .FirstOrDefault();

                        if (tournamentId != 0)
                        {
                            await _standingsUpdater.UpdateTeamStandingsAsync(tournamentId);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerEventExists(playerEvent.PlayerEventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index), new { matchId = matchId, tournamentRoundId = tournamentRoundId });
            }

            var match = _context.Matches
                .Where(m => m.MatchId == matchId)
                .Include(m => m.TeamA).ThenInclude(t => t.Players)
                .Include(m => m.TeamB).ThenInclude(t => t.Players)
                .FirstOrDefault();

            if (match == null) return NotFound();

            var players = match.TeamA.Players
                .Concat(match.TeamB.Players)
                .Distinct()
                .ToList();

            ViewBag.PlayerId = new SelectList(players, "PlayerId", "FullName");
            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(playerEvent);
        }


        public async Task<IActionResult> Delete(int? id, int? matchId, int? tournamentRoundId)
        {
            if (id == null || matchId == null)
            {
                return NotFound();
            }

            var playerEvent = await _context.PlayerEvents
                .Include(p => p.Match)
                .Include(p => p.Player)
                .FirstOrDefaultAsync(m => m.PlayerEventId == id && m.MatchId == matchId);

            if (playerEvent == null)
            {
                return NotFound();
            }

            ViewData["MatchId"] = matchId;
            ViewData["TournamentRoundId"] = tournamentRoundId;

            return View(playerEvent);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, int? matchId, int? tournamentRoundId)
        {
            var playerEvent = await _context.PlayerEvents
                .Include(pe => pe.Player)
                .FirstOrDefaultAsync(pe => pe.PlayerEventId == id);

            if (playerEvent != null)
            {
                if (playerEvent.Points != null)
                {
                    var matchRes = await _context.MatchResults
                        .FirstOrDefaultAsync(mr => mr.MatchId == matchId && mr.TeamId == playerEvent.Player.TeamId);

                    if (matchRes != null)
                    {
                        matchRes.Points -= playerEvent.Points.Value;
                    }
                }

                _context.PlayerEvents.Remove(playerEvent);
                await _context.SaveChangesAsync();

                if (matchId != null)
                {
                    var tournamentId = await _context.Matches
                        .Where(m => m.MatchId == matchId)
                        .Select(m => m.TournamentId)
                        .FirstOrDefaultAsync();

                    if (tournamentId != 0)
                    {
                        await _standingsUpdater.UpdateTeamStandingsAsync(tournamentId);
                    }
                }
            }

            return RedirectToAction(nameof(Index), new { matchId = matchId, tournamentRoundId = tournamentRoundId });
        }

        private bool PlayerEventExists(int id)
        {
            return _context.PlayerEvents.Any(e => e.PlayerEventId == id);
        }
    }
}
