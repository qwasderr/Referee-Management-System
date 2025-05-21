using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Helpers.SportSystem2.Helpers;
using SportSystem2.Migrations;
using SportSystem2.Models;
using SportSystem2.Services;

namespace SportSystem2.Controllers
{
    public class Players2Controller : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private const string UploadFolder = "players";
        public Players2Controller(ApplicationDbContext context, IWebHostEnvironment environment, IImageService imageService)
        {
            _context = context;
            _environment = environment;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Players.Include(p => p.Team);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var player = await _context.Players
                .Include(p => p.Team)
                .Include(p => p.PlayerEvents)
                    .ThenInclude(pe => pe.Match)
                        .ThenInclude(m => m.TeamA)
                .Include(p => p.PlayerEvents)
                    .ThenInclude(pe => pe.Match)
                        .ThenInclude(m => m.TeamB)
                .Include(p => p.PlayerEvents)
                    .ThenInclude(pe => pe.Match)
                        .ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null)
                return NotFound();

            return View(player);
        }

        public IActionResult Create()
        {
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("PlayerId,TeamId,FullName,Position,BirthDate,Height,Weight,Gender,Number")] Player player)
        {
            if (ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound();

            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerId,TeamId,FullName,Position,BirthDate,Height,Weight,Gender,PhotoPath,Number")] Player player, IFormFile? Photo)
        {
            if (id != player.PlayerId)
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
                try
                {
                    var existingPlayer = await _context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.PlayerId == id);
                    if (existingPlayer == null)
                        return NotFound();

                    if (Photo != null && Photo.Length > 0)
                    {
                        _imageService.DeleteImage(existingPlayer.PhotoPath, UploadFolder);
                        player.PhotoPath = await _imageService.SaveImageAsync(Photo, UploadFolder);
                    }
                    else
                    {
                        player.PhotoPath = existingPlayer.PhotoPath;
                    }

                    _context.Update(player);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.PlayerId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.PlayerId == id);

            if (player == null)
                return NotFound();

            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return RedirectToAction("Index", "Teams");
            }

            bool hasPlayerEvents = await _context.PlayerEvents.AnyAsync(pe => pe.PlayerId == id);

            if (hasPlayerEvents)
            {
                TempData["ErrorMessage"] = "Deletion is not allowed because the player has related events.";
                return RedirectToAction("Details", "Teams", new { id = player.TeamId });
            }

            if (!string.IsNullOrEmpty(player.PhotoPath))
            {
                _imageService.DeleteImage(player.PhotoPath, UploadFolder);
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = player.TeamId });
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.PlayerId == id);
        }
    }
}
