using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;
using System.ComponentModel.DataAnnotations;
using SportSystem2.Models.DTOs;

namespace SportSystem2.Controllers
{
    public class JudgesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public JudgesController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /*public class JudgeDTO
        {
            public int JudgeId { get; set; }
            public string FullName { get; set; } = null!;
            public string QualificationLevel { get; set; } = null!;
            public DateTime? LastAttestationDate { get; set; }
            public string AvatarUrl { get; set; } = "/images/default-avatar.png";
        }*/
       
        public async Task<IActionResult> Index()
        {
            var judges = await _context.Judges
                .Include(j => j.ApplicationUser)
                .Select(j => new JudgeDTO
                {
                    JudgeId = j.JudgeId,
                    FullName = j.FullName,
                    QualificationLevel = j.QualificationLevel,
                    LastAttestationDate = j.LastAttestationDate,
                    AvatarUrl = j.ApplicationUser.PhotoUrl ?? "/images/default-avatar.png"
                })
                .ToListAsync();

            return View(judges);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var judge = await _context.Judges
                .Include(j => j.ApplicationUser)
                .Include(j => j.GameAssignments)
                    .ThenInclude(ga => ga.Match)
                        .ThenInclude(m => m.TeamA)
                .Include(j => j.GameAssignments)
                    .ThenInclude(ga => ga.Match)
                        .ThenInclude(m => m.TeamB)
                .Include(j => j.GameAssignments)
                    .ThenInclude(ga => ga.Match)
                        .ThenInclude(m => m.Tournament)
                .Include(j => j.GameAssignments)
                    .ThenInclude(ga => ga.Match)
                        .ThenInclude(m => m.TournamentRound)
                .Include(j => j.MatchAnalyses)
                    .ThenInclude(ma => ma.Match)
                        .ThenInclude(m => m.TeamA)
                .Include(j => j.MatchAnalyses)
                    .ThenInclude(ma => ma.Match)
                        .ThenInclude(m => m.TeamB)
                .Where(j => j.JudgeId == id)
                .Select(j => new JudgeDTO
                {
                    JudgeId = j.JudgeId,
                    FullName = j.FullName,
                    QualificationLevel = j.QualificationLevel,
                    LastAttestationDate = j.LastAttestationDate,
                    AvatarUrl = j.ApplicationUser.PhotoUrl ?? "/images/default-avatar.png",
                    GameAssignments = j.GameAssignments,
                    MatchAnalyses = j.MatchAnalyses
                })
                .FirstOrDefaultAsync();

            if (judge == null) return NotFound();

            return View(judge);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(JudgeCreateDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhotoUrl = "/images/default-avatar.png"
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync("Judge"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Judge"));
            }
            await _userManager.AddToRoleAsync(user, "Judge");

            var judge = new Judge
            {
                ApplicationUserId = user.Id,
                FullName = model.FullName,
                QualificationLevel = model.QualificationLevel,
                LastAttestationDate = null
            };

            _context.Judges.Add(judge);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var judge = await _context.Judges.FindAsync(id);
            if (judge == null) return NotFound();

            var model = new JudgeEditDTO
            {
                JudgeId = judge.JudgeId,
                FullName = judge.FullName,
                QualificationLevel = judge.QualificationLevel,
                LastAttestationDate = judge.LastAttestationDate,
                ApplicationUserId = judge.ApplicationUserId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, JudgeEditDTO model)
        {
            if (id != model.JudgeId) return NotFound();

            if (!ModelState.IsValid) return View(model);

            var judge = await _context.Judges.FindAsync(id);
            if (judge == null) return NotFound();

            judge.FullName = model.FullName;
            judge.QualificationLevel = model.QualificationLevel;
            judge.LastAttestationDate = model.LastAttestationDate;

            try
            {
                _context.Update(judge);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JudgeExists(judge.JudgeId)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var judge = await _context.Judges
                .Include(j => j.ApplicationUser)
                .Where(j => j.JudgeId == id)
                .Select(j => new JudgeDTO
                {
                    JudgeId = j.JudgeId,
                    FullName = j.FullName,
                    QualificationLevel = j.QualificationLevel,
                    LastAttestationDate = j.LastAttestationDate,
                    AvatarUrl = j.ApplicationUser.PhotoUrl ?? "/images/default-avatar.png"
                })
                .FirstOrDefaultAsync();

            if (judge == null) return NotFound();

            return View(judge);
        }

        /*[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var judge = await _context.Judges.FindAsync(id);
            if (judge != null)
            {
                _context.Judges.Remove(judge);

                var user = await _userManager.FindByIdAsync(judge.ApplicationUserId);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Не вдалося видалити користувача.");
                        return View(judge);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var judge = await _context.Judges
                .Include(j => j.Matches)            
                .Include(j => j.MatchAnalyses)      
                .Include(j => j.GameAssignments)    
                .Include(j => j.TestResults)        
                .FirstOrDefaultAsync(j => j.JudgeId == id);

            if (judge == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if ((judge.Matches != null && judge.Matches.Any()) ||
                (judge.MatchAnalyses != null && judge.MatchAnalyses.Any()) ||
                (judge.GameAssignments != null && judge.GameAssignments.Any()))
            {
                ModelState.AddModelError("", "Cannot delete judge with existing matches, match analyses, or game assignments.");
                return View(judge);
            }

            if (judge.TestResults != null && judge.TestResults.Any())
            {
                _context.TestResults.RemoveRange(judge.TestResults);
            }

            _context.Judges.Remove(judge);

            var user = await _userManager.FindByIdAsync(judge.ApplicationUserId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to delete the associated user account.");
                    return View(judge);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JudgeExists(int id)
        {
            return _context.Judges.Any(e => e.JudgeId == id);
        }
    }

}
