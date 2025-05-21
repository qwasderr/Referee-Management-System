using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Areas.Identity.Pages.Account.Manage
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public ProfileModel(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string AvatarUrl { get; set; }

        public string QualificationLevel { get; set; }

        public bool IsJudge { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            Input = new InputModel
            {
                FullName = user.FullName
            };

            AvatarUrl = user.PhotoUrl;
            IsJudge = await _userManager.IsInRoleAsync(user, "Judge");

            if (IsJudge)
            {
                var judge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == user.Id);
                QualificationLevel = judge?.QualificationLevel ?? "Unknown";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile AvatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            IsJudge = await _userManager.IsInRoleAsync(user, "Judge");

            if (!ModelState.IsValid)
            {
                AvatarUrl = user.PhotoUrl;

                if (IsJudge)
                {
                    var judge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == user.Id);
                    QualificationLevel = judge?.QualificationLevel ?? "Unknown";
                }

                return Page();
            }

            bool hasChanges = false;

            if (user.FullName != Input.FullName)
            {
                user.FullName = Input.FullName;
                hasChanges = true;
            }

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{user.Id}{Path.GetExtension(AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                user.PhotoUrl = $"/avatars/{fileName}";
                hasChanges = true;
            }

            if (hasChanges)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    AvatarUrl = user.PhotoUrl;

                    if (IsJudge)
                    {
                        var judge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == user.Id);
                        QualificationLevel = judge?.QualificationLevel ?? "Unknown";
                    }

                    return Page();
                }

                StatusMessage = "Your profile has been updated";
            }
            else
            {
                StatusMessage = "No changes were made.";
            }

            return RedirectToPage();
        }
    }
}
