using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Helpers.SportSystem2.Helpers;
using SportSystem2.Models;
using SportSystem2.Services;

namespace SportSystem2.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;
        private readonly string UploadFolder = "teams";
        public TeamsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IImageService imageService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Teams.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
     .Include(t => t.Players)
     .Include(t => t.MatchResults)
         .ThenInclude(mr => mr.Match)
             .ThenInclude(m => m.TeamA)
     .Include(t => t.MatchResults)
         .ThenInclude(mr => mr.Match)
             .ThenInclude(m => m.TeamB)
     .Include(t => t.MatchResults)
         .ThenInclude(mr => mr.Match)
             .ThenInclude(m => m.MatchResults)
     .Include(t => t.MatchResults)
         .ThenInclude(mr => mr.Match)
             .ThenInclude(m => m.Tournament)
     .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Team team, IFormFile? Photo)
        {
            if (ImageValidator.FileIsNull(Photo))
            {
                ModelState.AddModelError("", "Please select a photo to upload.");
                return View();
            }

            if (!ImageValidator.IsValidContentType(Photo.ContentType))
            {
                ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
                return View();
            }
            if (ModelState.IsValid)
            {
                if (Photo != null)
                {
                    var savedPath = await _imageService.SaveImageAsync(Photo, UploadFolder);
                    team.PhotoPath = savedPath;
                }

                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(team);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Team team, IFormFile? Photo)
        {
            if (id != team.TeamId)
                return NotFound();
            if (ImageValidator.FileIsNull(Photo))
            {
                ModelState.AddModelError("", "Please select a photo to upload.");
                return View();
            }

            if (!ImageValidator.IsValidContentType(Photo.ContentType))
            {
                ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
                return View();
            }
            if (ModelState.IsValid)
            {
                if (Photo != null)
                {
                    _imageService.DeleteImage(team.PhotoPath, UploadFolder);
                    var newPath = await _imageService.SaveImageAsync(Photo, UploadFolder);
                    team.PhotoPath = newPath;
                }


                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Teams.Any(e => e.TeamId == team.TeamId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        /*[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            bool hasMatches = await _context.Matches
                .AnyAsync(m => m.TeamAId == id || m.TeamBId == id);

            if (hasMatches)
            {
                ModelState.AddModelError(string.Empty, "Removal is not possible because the team has related matches.");
                return View(team);
            }
            _imageService.DeleteImage(team.PhotoPath, UploadFolder);
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamId == id);
        }
    }
}
