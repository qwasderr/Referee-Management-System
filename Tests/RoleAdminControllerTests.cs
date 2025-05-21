using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;

namespace Tests
{
    public class RoleAdminControllerTests
    {
        private Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateRole_ShouldReturnRedirect_WhenRoleIsCreated()
        {
            var roleName = "Judge";
            var roleManager = GetMockRoleManager();
            roleManager.Setup(r => r.RoleExistsAsync(roleName)).ReturnsAsync(false);
            roleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            var controller = new RoleAdminController(roleManager.Object, GetMockUserManager().Object, GetInMemoryDbContext());
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            var result = await controller.CreateRole(roleName);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListRoles", redirect.ActionName);
        }

        [Fact]
        public async Task CreateRole_ShouldReturnView_WhenRoleAlreadyExists()
        {
            var roleName = "Admin";
            var roleManager = GetMockRoleManager();
            roleManager.Setup(r => r.RoleExistsAsync(roleName)).ReturnsAsync(true);

            var controller = new RoleAdminController(roleManager.Object, GetMockUserManager().Object, GetInMemoryDbContext());

            var result = await controller.CreateRole(roleName);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DeleteRole_ShouldDeleteRole_WhenNoUsersAssigned()
        {
            var roleName = "Referee";
            var roleManager = GetMockRoleManager();
            var role = new IdentityRole(roleName);
            roleManager.Setup(r => r.FindByNameAsync(roleName)).ReturnsAsync(role);
            roleManager.Setup(r => r.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

            var userManager = GetMockUserManager();
            userManager.Setup(u => u.GetUsersInRoleAsync(roleName)).ReturnsAsync(new List<ApplicationUser>());

            var controller = new RoleAdminController(roleManager.Object, userManager.Object, GetInMemoryDbContext());
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            var result = await controller.DeleteRole(roleName);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListRoles", redirect.ActionName);
        }

        [Fact]
        public async Task AddRoleToUser_ShouldReturnError_WhenUserNotFound()
        {
            var model = new UserRoleViewModel
            {
                UserName = "nonexistent",
                SelectedRole = "Judge"
            };

            var roleManager = GetMockRoleManager();
            roleManager.Setup(r => r.RoleExistsAsync("Judge")).ReturnsAsync(true);

            var userManager = GetMockUserManager();
            userManager.Setup(u => u.FindByNameAsync("nonexistent")).ReturnsAsync((ApplicationUser)null);

            var controller = new RoleAdminController(roleManager.Object, userManager.Object, GetInMemoryDbContext());

            var result = await controller.AddRoleToUser(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task RemoveRoleFromUser_ShouldRemoveRoleAndDeleteJudge_WhenUserHasJudgeRole()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            using (var db = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "123", UserName = "testuser", FullName = "Test User" };
                db.Users.Add(user);
                db.Judges.Add(new Judge
                {
                    ApplicationUserId = user.Id,
                    FullName = user.FullName,
                    QualificationLevel = "Unqualified"
                });
                await db.SaveChangesAsync();

                var model = new UserRoleViewModel
                {
                    UserName = "testuser",
                    SelectedRole = "Judge"
                };

                var roleManager = GetMockRoleManager();
                roleManager.Setup(r => r.RoleExistsAsync("Judge")).ReturnsAsync(true);

                var userManager = GetMockUserManager();
                userManager.Setup(u => u.FindByNameAsync("testuser")).ReturnsAsync(user);
                userManager.Setup(u => u.IsInRoleAsync(user, "Judge")).ReturnsAsync(true);
                userManager.Setup(u => u.RemoveFromRoleAsync(user, "Judge")).ReturnsAsync(IdentityResult.Success);

                var controller = new RoleAdminController(roleManager.Object, userManager.Object, db);

                var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData = tempData;

                var result = await controller.RemoveRoleFromUser(model);

                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("ListRoles", redirect.ActionName);

                var judgeStillExists = await db.Judges.AnyAsync(j => j.ApplicationUserId == user.Id);
                Assert.False(judgeStillExists);

                Assert.Equal("Role 'Judge' removed from user 'testuser'.", controller.TempData["Message"]);
            }
        }
    }
}
