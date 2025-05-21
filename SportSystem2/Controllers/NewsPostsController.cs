using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Helpers.SportSystem2.Helpers;
using SportSystem2.Models;
using SportSystem2.Services;

namespace SportSystem2.Controllers
{
    public class NewsPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly string UploadFolder = "news";
        public NewsPostsController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.NewsPosts.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var newsPost = await _context.NewsPosts.FirstOrDefaultAsync(m => m.NewsPostId == id);
            if (newsPost == null) return NotFound();

            return View(newsPost);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Content")] NewsPost newsPost, IFormFile? Photo)
        {
            /*if (ImageValidator.FileIsNull(Photo))
            {
                ModelState.AddModelError("", "Please select a photo to upload.");
                return View();
            }*/


            if (ModelState.IsValid)
            {
                newsPost.CreatedAt = DateTime.Now;

                if (Photo != null && Photo.Length > 0)
                {
                    if (!ImageValidator.IsValidContentType(Photo.ContentType))
                    {
                        ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
                        return View();
                    }
                    var photoPath = await _imageService.SaveImageAsync(Photo, UploadFolder);
                    newsPost.PhotoPath = photoPath;
                }

                _context.Add(newsPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newsPost);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var newsPost = await _context.NewsPosts.FindAsync(id);
            if (newsPost == null) return NotFound();

            return View(newsPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("NewsPostId,Title,Content,CreatedAt,PhotoPath")] NewsPost newsPost, IFormFile? Photo)
        {
            /*if (ImageValidator.FileIsNull(Photo))
            {
                ModelState.AddModelError("", "Please select a photo to upload.");
                return View();
            }*/


            if (id != newsPost.NewsPostId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (Photo != null && Photo.Length > 0)
                    {
                        if (!ImageValidator.IsValidContentType(Photo.ContentType))
                        {
                            ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
                            return View();
                        }
                        if (!string.IsNullOrEmpty(newsPost.PhotoPath))
                        {
                            _imageService.DeleteImage(newsPost.PhotoPath, UploadFolder);
                        }

                        var photoPath = await _imageService.SaveImageAsync(Photo, UploadFolder);
                        newsPost.PhotoPath = photoPath;
                    }

                    _context.Update(newsPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsPostExists(newsPost.NewsPostId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(newsPost);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var newsPost = await _context.NewsPosts
                .FirstOrDefaultAsync(m => m.NewsPostId == id);
            if (newsPost == null) return NotFound();

            return View(newsPost);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var newsPost = await _context.NewsPosts.FindAsync(id);
            if (newsPost != null)
            {
                if (!string.IsNullOrEmpty(newsPost.PhotoPath))
                {
                    _imageService.DeleteImage(newsPost.PhotoPath, UploadFolder);
                }

                _context.NewsPosts.Remove(newsPost);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NewsPostExists(int id)
        {
            return _context.NewsPosts.Any(e => e.NewsPostId == id);
        }
    }
}
