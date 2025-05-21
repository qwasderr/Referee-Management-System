using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using System.Data;

namespace SportSystem2.Controllers
{

    public class TestResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetNotCheckedTestsCount()
        {
            var count = _context.TestResults.Count(tr => tr.Score == null);
            return Json(new { notCheckedCount = count });
        }

        public async Task<IActionResult> Index()
        {

            var testResults = await _context.TestResults
    .Include(tr => tr.Judge)
    .Include(tr => tr.Test)
    .OrderBy(tr => tr.Score.HasValue)
    .ThenByDescending(tr => tr.DateTaken)
    .ToListAsync();
            return View(testResults);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var testResult = await _context.TestResults
                .Include(t => t.Judge)
                .Include(t => t.Test)
                .FirstOrDefaultAsync(m => m.TestResultId == id);

            if (testResult == null) return NotFound();

            return View(testResult);
        }

        public IActionResult Create()
        {
            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName");
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestUrl");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TestResultId,JudgeId,TestId,Score,DateTaken,Explanation")] TestResult testResult)
        {
            if (ModelState.IsValid)
            {
                _context.Add(testResult);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", testResult.JudgeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestUrl", testResult.TestId);
            return View(testResult);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null) return NotFound();

            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", testResult.JudgeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestUrl", testResult.TestId);
            return View(testResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TestResultId,JudgeId,TestId,Score,DateTaken,Explanation")] TestResult testResult)
        {
            if (id != testResult.TestResultId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testResult);

                    var judge = await _context.Judges.FindAsync(testResult.JudgeId);
                    if (judge != null)
                    {
                        judge.LastAttestationDate = testResult.DateTaken;


                        _context.Entry(judge).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestResultExists(testResult.TestResultId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["JudgeId"] = new SelectList(_context.Judges, "JudgeId", "FullName", testResult.JudgeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestUrl", testResult.TestId);
            return View(testResult);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var testResult = await _context.TestResults
                .Include(t => t.Judge)
                .Include(t => t.Test)
                .FirstOrDefaultAsync(m => m.TestResultId == id);
            if (testResult == null) return NotFound();

            return View(testResult);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult != null)
            {
                _context.TestResults.Remove(testResult);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TestResultExists(int id)
        {
            return _context.TestResults.Any(e => e.TestResultId == id);
        }
    }
}
