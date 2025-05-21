using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using SportSystem2.Services;

public class TournamentRoundsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly StandingsUpdater _standingsUpdater;
    public TournamentRoundsController(ApplicationDbContext context, StandingsUpdater standingsUpdater)
    {
        _context = context;
        _standingsUpdater = standingsUpdater;
    }

    public async Task<IActionResult> Index(int? tournamentId)
    {
        IQueryable<TournamentRound> query = _context.TournamentRounds.Include(t => t.Tournament);

        if (tournamentId.HasValue)
        {
            query = query.Where(tr => tr.TournamentId == tournamentId.Value);
            ViewData["TournamentId"] = tournamentId.Value;

            var tournament = await _context.Tournaments.FindAsync(tournamentId.Value);
            if (tournament != null)
            {
                ViewData["TournamentName"] = tournament.Name;
            }
        }

        var rounds = await query.ToListAsync();
        return View(rounds);
    }

    public async Task<IActionResult> Details(int? id, int? tournamentId)
    {
        if (id == null)
            return NotFound();

        var tournamentRound = await _context.TournamentRounds
            .Include(t => t.Tournament)
            .FirstOrDefaultAsync(m => m.RoundId == id);

        if (tournamentRound == null)
            return NotFound();

        ViewData["TournamentId"] = tournamentId ?? tournamentRound.TournamentId;

        return View(tournamentRound);
    }

    public IActionResult Create(int? tournamentId)
    {
        if (tournamentId.HasValue)
        {
            ViewData["TournamentId"] = new SelectList(_context.Tournaments.Where(t => t.TournamentId == tournamentId.Value), "TournamentId", "Name", tournamentId.Value);
            ViewData["CurrentTournamentId"] = tournamentId.Value;
        }
        else
        {
            ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name");
            ViewData["CurrentTournamentId"] = null;
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("RoundId,TournamentId,RoundName,Location,StartDate,EndDate")] TournamentRound tournamentRound)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tournamentRound);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { tournamentId = tournamentRound.TournamentId });
        }
        ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name", tournamentRound.TournamentId);
        ViewData["CurrentTournamentId"] = tournamentRound.TournamentId;
        return View(tournamentRound);
    }

    public async Task<IActionResult> Edit(int? id, int? tournamentId)
    {
        if (id == null)
            return NotFound();

        var tournamentRound = await _context.TournamentRounds.FindAsync(id);
        if (tournamentRound == null)
            return NotFound();

        ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name", tournamentRound.TournamentId);
        ViewData["CurrentTournamentId"] = tournamentId ?? tournamentRound.TournamentId;

        return View(tournamentRound);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("RoundId,TournamentId,RoundName,Location,StartDate,EndDate")] TournamentRound tournamentRound, int? tournamentId)
    {
        if (id != tournamentRound.RoundId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tournamentRound);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TournamentRoundExists(tournamentRound.RoundId))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index), new { tournamentId = tournamentRound.TournamentId });
        }
        ViewData["TournamentId"] = new SelectList(_context.Tournaments, "TournamentId", "Name", tournamentRound.TournamentId);
        ViewData["CurrentTournamentId"] = tournamentId ?? tournamentRound.TournamentId;
        return View(tournamentRound);
    }

    public async Task<IActionResult> Delete(int? id, int? tournamentId)
    {
        if (id == null)
            return NotFound();

        var tournamentRound = await _context.TournamentRounds
            .Include(t => t.Tournament)
            .FirstOrDefaultAsync(m => m.RoundId == id);

        if (tournamentRound == null)
            return NotFound();

        ViewData["TournamentId"] = tournamentId ?? tournamentRound.TournamentId;

        return View(tournamentRound);
    }

    /*[HttpPost, ActionName("Delete")]
     [ValidateAntiForgeryToken]
     [Authorize(Roles = "Admin")]
     public async Task<IActionResult> DeleteConfirmed(int id, int? tournamentId)
     {
         var tournamentRound = await _context.TournamentRounds.FindAsync(id);
         if (tournamentRound != null)
         {
             tournamentId ??= tournamentRound.TournamentId;

             _context.TournamentRounds.Remove(tournamentRound);
             await _context.SaveChangesAsync();
         }
         return RedirectToAction(nameof(Index), new { tournamentId });
     }*/
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id, int? tournamentId)
    {
        var tournamentRound = await _context.TournamentRounds
            .Include(tr => tr.Matches)
                .ThenInclude(m => m.GameAssignments)
            .FirstOrDefaultAsync(tr => tr.RoundId == id);

        if (tournamentRound != null)
        {
            tournamentId ??= tournamentRound.TournamentId;

            foreach (var match in tournamentRound.Matches)
            {
                _context.GameAssignments.RemoveRange(match.GameAssignments);
            }
            _context.Matches.RemoveRange(tournamentRound.Matches);

            _context.TournamentRounds.Remove(tournamentRound);

            await _context.SaveChangesAsync();
            await _standingsUpdater.UpdateTeamStandingsAsync(tournamentRound.TournamentId);
        }

        return RedirectToAction(nameof(Index), new { tournamentId });
    }

    private bool TournamentRoundExists(int id)
    {
        return _context.TournamentRounds.Any(e => e.RoundId == id);
    }
}
