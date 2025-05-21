using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using System.Security.Claims;

namespace SportSystem2.Controllers
{
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int testId)
        {
            var test = await _context.Tests.FindAsync(testId);
            if (test == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var judge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == userId);
            if (judge == null)
                return NotFound("Judge not found for the current user.");

            var result = new TestResult
            {
                TestId = testId,
                DateTaken = DateTime.Now,
                JudgeId = judge.JudgeId
            };

            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Tests");
        }



        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var judge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == userId);
            var tests = await _context.Tests.ToListAsync();
            List<TestWithStatusViewModel> model = new List<TestWithStatusViewModel>();
            if (judge != null)
            {
                var completedTestIds = await _context.TestResults
               .Where(tr => tr.JudgeId == judge.JudgeId)
               .Select(tr => tr.TestId)
               .ToListAsync();

                model = tests.Select(t => new TestWithStatusViewModel
                {
                    Test = t,
                    IsCompleted = completedTestIds.Contains(t.TestId)
                }).ToList();
            }
            else
            {
                model = tests.Select(t => new TestWithStatusViewModel
                {
                    Test = t,
                    IsCompleted = false
                }).ToList();
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TestId,Title,TestUrl")] Test test)
        {
            if (ModelState.IsValid)
            {
                _context.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TestId,Title,TestUrl")] Test test)
        {
            if (id != test.TestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(test);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.TestId))
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
            return View(test);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests
                .Include(t => t.TestResults)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test != null)
            {
                _context.TestResults.RemoveRange(test.TestResults);

                _context.Tests.Remove(test);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}
