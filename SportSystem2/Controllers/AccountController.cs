using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportSystem2.Helpers.SportSystem2.Helpers;
using SportSystem2.Models;
using SportSystem2.Services;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageService _imageService;
    private readonly string UploadFolder = "users";

    public AccountController(UserManager<ApplicationUser> userManager, IImageService imageService)
    {
        _userManager = userManager;
        _imageService = imageService;
    }

    [HttpGet]
    public IActionResult UploadPhoto()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        if (ImageValidator.FileIsNull(photo))
        {
            ModelState.AddModelError("", "Please select a photo to upload.");
            return View();
        }

        if (!ImageValidator.IsValidContentType(photo.ContentType))
        {
            ModelState.AddModelError("", "Only JPG, PNG, and GIF images are allowed.");
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }


        if (!string.IsNullOrEmpty(user.PhotoUrl))
        {
            _imageService.DeleteImage(user.PhotoUrl, UploadFolder);
        }

        var photoPath = await _imageService.SaveImageAsync(photo, UploadFolder);
        user.PhotoUrl = photoPath;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            ModelState.AddModelError("", "Failed to update user profile.");
            return View();
        }

        return RedirectToAction("Profile");
    }
}
