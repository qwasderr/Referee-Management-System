using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;

namespace SportSystem2.Controllers
{
    public class MatchAnalysisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchAnalysisController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["MatchesDescriptions"] = GetMatchSelectList();

            var applicationDbContext = _context.MatchAnalyses
                .Include(m => m.CreatedByJudge)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(m => m.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TournamentRound);

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["MatchesDescriptions"] = GetMatchSelectList(id);

            var matchAnalysis = await _context.MatchAnalyses
                .Include(m => m.CreatedByJudge)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(m => m.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.MatchAnalysisId == id);

            if (matchAnalysis == null)
            {
                return NotFound();
            }

            return View(matchAnalysis);
        }

        public async Task<IActionResult> Create(int? matchId)
        {
            var userId = _userManager.GetUserId(User);

            var judge = await _context.Judges
                .FirstOrDefaultAsync(j => j.ApplicationUserId == userId);

            if (judge == null)
                return Forbid();

            ViewData["MatchesDescriptions"] = GetMatchSelectList();
            ViewData["CreatedByJudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", judge.JudgeId);
            ViewData["MatchId"] = matchId;
            ViewData["JudgeId"] = judge.JudgeId;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MatchId,Title,Content,CreatedByJudgeId,AttachmentUrl,MinuteFrom,MinuteTo")] MatchAnalysis matchAnalysis, int? matchId)
        {
            ViewData["MatchesDescriptions"] = GetMatchSelectList();

            if (ModelState.IsValid)
            {
                matchAnalysis.CreatedAt = DateTime.Now;
                matchAnalysis.LastEditedAt = DateTime.Now;

                _context.Add(matchAnalysis);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedByJudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", matchAnalysis.CreatedByJudgeId);
            if (matchId == null) ViewData["MatchId"] = GetMatchSelectList(matchAnalysis.MatchId);
            else ViewData["MatchId"] = matchId.ToString();
            return View(matchAnalysis);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["MatchesDescriptions"] = GetMatchSelectList(id);

            var matchAnalysis = await _context.MatchAnalyses.FindAsync(id);
            if (matchAnalysis == null)
            {
                return NotFound();
            }
            ViewData["CreatedByJudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", matchAnalysis.CreatedByJudgeId);
            ViewData["MatchId"] = GetMatchSelectList(matchAnalysis.MatchId);
            return View(matchAnalysis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MatchAnalysisId,MatchId,Title,Content,CreatedByJudgeId,AttachmentUrl,MinuteFrom,MinuteTo")] MatchAnalysis matchAnalysis)
        {
            ViewData["MatchesDescriptions"] = GetMatchSelectList(id);

            if (id != matchAnalysis.MatchAnalysisId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAnalysis = await _context.MatchAnalyses.AsNoTracking()
                        .FirstOrDefaultAsync(m => m.MatchAnalysisId == id);

                    if (existingAnalysis == null)
                        return NotFound();

                    matchAnalysis.CreatedAt = existingAnalysis.CreatedAt;
                    matchAnalysis.LastEditedAt = DateTime.Now;

                    _context.Update(matchAnalysis);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchAnalysisExists(matchAnalysis.MatchAnalysisId))
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
            ViewData["CreatedByJudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", matchAnalysis.CreatedByJudgeId);
            ViewData["MatchId"] = GetMatchSelectList(matchAnalysis.MatchId);
            return View(matchAnalysis);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["MatchesDescriptions"] = GetMatchSelectList(id);

            var matchAnalysis = await _context.MatchAnalyses
                .Include(m => m.CreatedByJudge)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(m => m.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(m => m.MatchAnalysisId == id);
            if (matchAnalysis == null)
            {
                return NotFound();
            }

            return View(matchAnalysis);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var matchAnalysis = await _context.MatchAnalyses.FindAsync(id);
            if (matchAnalysis != null)
            {
                _context.MatchAnalyses.Remove(matchAnalysis);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatchAnalysisExists(int id)
        {
            return _context.MatchAnalyses.Any(e => e.MatchAnalysisId == id);
        }

        private SelectList GetMatchSelectList(int? selectedMatchId = null)
        {
            var matchList = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Tournament)
                .Include(m => m.TournamentRound)
                .Select(m => new
                {
                    m.MatchId,
                    Description = m.TeamA.Name + " - " + m.TeamB.Name + " | " +
                                  m.Tournament.Name + " | " +
                                  m.TournamentRound.RoundName
                })
                .ToList();

            return new SelectList(matchList, "MatchId", "Description", selectedMatchId);
        }

        public async Task<IActionResult> MyAnalyses()
        {
            var userId = _userManager.GetUserId(User);
            var judge = await _context.Judges
                .FirstOrDefaultAsync(j => j.ApplicationUserId == userId);

            if (judge == null)
            {
                return Forbid();
            }

            ViewData["MatchesDescriptions"] = GetMatchSelectList();

            var myAnalyses = _context.MatchAnalyses
                .Where(m => m.CreatedByJudgeId == judge.JudgeId)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(m => m.Match)
                    .ThenInclude(m => m.Tournament)
                .Include(m => m.Match)
                    .ThenInclude(m => m.TournamentRound)
                .Include(m => m.CreatedByJudge);

            return View("IndexPers", await myAnalyses.ToListAsync());
        }
    }
}
