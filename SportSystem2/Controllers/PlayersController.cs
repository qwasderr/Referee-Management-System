using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Helpers.SportSystem2.Helpers;
using SportSystem2.Models;
using SportSystem2.Services;

namespace SportSystem2.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private const string UploadFolder = "players";

        public PlayersController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var players = _context.Players.Include(p => p.Team);
            return View(await players.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Team)
                .Include(p => p.PlayerEvents).ThenInclude(pe => pe.Match).ThenInclude(m => m.TeamA)
                .Include(p => p.PlayerEvents).ThenInclude(pe => pe.Match).ThenInclude(m => m.TeamB)
                .Include(p => p.PlayerEvents).ThenInclude(pe => pe.Match).ThenInclude(m => m.TournamentRound)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            return player == null ? NotFound() : View(player);
        }

        public IActionResult Create(int teamId)
        {
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", teamId);
            ViewBag.TeamId = teamId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("PlayerId,TeamId,FullName,Position,BirthDate,Height,Weight,Gender,Number")] Player player, IFormFile? Photo)
        {
            /*if (ImageValidator.FileIsNull(Photo))
            {
                ModelState.AddModelError("", "Please select a photo to upload.");
                return View();
            }*/

            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    if (!ImageValidator.IsValidContentType(Photo.ContentType))
                    {
                        ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
                        return View();
                    }
                    player.PhotoPath = await _imageService.SaveImageAsync(Photo, UploadFolder);

                }
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Teams", new { id = player.TeamId });
            }

            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerId,TeamId,FullName,Position,BirthDate,Height,Weight,Gender,PhotoPath,Number")] Player player, IFormFile? Photo)
        {
            if (id != player.PlayerId) return NotFound();
            /*if (ImageValidator.FileIsNull(Photo))
            {
                ModelState.AddModelError("", "Please select a photo to upload.");
                return View();
            }*/


            if (ModelState.IsValid)
            {
                try
                {
                    var existingPlayer = await _context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.PlayerId == id);
                    if (existingPlayer == null) return NotFound();

                    if (Photo != null && Photo.Length > 0)
                    {
                        if (!ImageValidator.IsValidContentType(Photo.ContentType))
                        {
                            ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
                            return View();
                        }
                        _imageService.DeleteImage(existingPlayer.PhotoPath, UploadFolder);
                        player.PhotoPath = await _imageService.SaveImageAsync(Photo, UploadFolder);
                    }
                    else
                    {
                        player.PhotoPath = existingPlayer.PhotoPath;
                    }

                    _context.Update(player);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Teams", new { id = player.TeamId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.PlayerId)) return NotFound();
                    throw;
                }
            }

            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players.Include(p => p.Team).FirstOrDefaultAsync(p => p.PlayerId == id);
            return player == null ? NotFound() : View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return RedirectToAction("Index", "Teams");

            bool hasPlayerEvents = await _context.PlayerEvents.AnyAsync(pe => pe.PlayerId == id);
            if (hasPlayerEvents)
            {
                TempData["ErrorMessage"] = "Deletion is not allowed because the player has related events.";
                return RedirectToAction("Details", "Teams", new { id = player.TeamId });
            }

            _imageService.DeleteImage(player.PhotoPath, UploadFolder);

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Teams", new { id = player.TeamId });
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(p => p.PlayerId == id);
        }
    }
}
