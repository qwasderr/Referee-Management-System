using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;

namespace SportSystem2.Controllers
{
    [Authorize(Roles = "Judge,Admin")]
    public class TestResultsViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestResultsViewController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var testResults = await _context.TestResults
                .Include(tr => tr.Judge)
                .Include(tr => tr.Test)
                .OrderByDescending(tr => tr.DateTaken)
                .ToListAsync();

            return View(testResults);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var testResult = await _context.TestResults
                .Include(tr => tr.Judge)
                .Include(tr => tr.Test)
                .FirstOrDefaultAsync(m => m.TestResultId == id);

            if (testResult == null) return NotFound();

            return View(testResult);
        }
    }
}
