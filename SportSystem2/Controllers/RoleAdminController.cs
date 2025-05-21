using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;

namespace SportSystem2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleAdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RoleAdminController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> ListRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Role name cannot be empty");
                return View();
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                ModelState.AddModelError("", "Role already exists");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (result.Succeeded)
            {
                TempData["Message"] = $"Role '{roleName}' created successfully";
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        [HttpGet]
        public IActionResult AddRoleToUser()
        {
            var model = new UserRoleViewModel
            {
                Roles = GetAllRoles(),
                Users = GetAllUsers()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRoleToUser(UserRoleViewModel model)
        {
            model.Users = GetAllUsers();
            model.Roles = GetAllRoles();

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                ModelState.AddModelError("", "Role does not exist");
                return View(model);
            }

            if (await _userManager.IsInRoleAsync(user, model.SelectedRole))
            {
                ModelState.AddModelError("SelectedRole", $"User '{model.UserName}' already has the role '{model.SelectedRole}'.");
                return View(model);
            }

            var result = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (result.Succeeded)
            {
                if (model.SelectedRole == "Judge")
                {
                    var existingJudge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == user.Id);
                    if (existingJudge == null)
                    {
                        var fullName = user.FullName;

                        _context.Judges.Add(new Judge
                        {
                            ApplicationUserId = user.Id,
                            FullName = fullName,
                            QualificationLevel = "Unqualified"
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Message"] = $"Role '{model.SelectedRole}' assigned to user '{model.UserName}'.";
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        public async Task<IActionResult> ListUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Role name is required for deletion.";
                return RedirectToAction(nameof(ListRoles));
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(ListRoles));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Any())
            {
                TempData["Error"] = "Cannot delete role because it is assigned to users.";
                return RedirectToAction(nameof(ListRoles));
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                TempData["Message"] = $"Role '{roleName}' deleted successfully.";
            else
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ListRoles));
        }

        #region Private Helpers

        private List<string> GetAllRoles()
        {
            return _roleManager.Roles.Select(r => r.Name).ToList();
        }

        private List<string> GetAllUsers()
        {
            return _userManager.Users.Select(u => u.UserName).ToList();
        }

        [HttpGet]
        public IActionResult RemoveRoleFromUser()
        {
            var model = new UserRoleViewModel
            {
                Users = GetAllUsers(),
                Roles = GetAllRoles()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRoleFromUser(UserRoleViewModel model)
        {
            model.Users = GetAllUsers();
            model.Roles = GetAllRoles();

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                ModelState.AddModelError("", "Role does not exist");
                return View(model);
            }

            if (!await _userManager.IsInRoleAsync(user, model.SelectedRole))
            {
                ModelState.AddModelError("", $"User '{model.UserName}' does not have the role '{model.SelectedRole}'.");
                return View(model);
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.SelectedRole);
            if (result.Succeeded)
            {
                if (model.SelectedRole == "Judge")
                {
                    var existingJudge = await _context.Judges.FirstOrDefaultAsync(j => j.ApplicationUserId == user.Id);
                    if (existingJudge != null)
                    {
                        _context.Judges.Remove(existingJudge);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Message"] = $"Role '{model.SelectedRole}' removed from user '{model.UserName}'.";
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        #endregion
    }
}
