using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Controllers;
using SportSystem2.Data;
using SportSystem2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Tests
{
    public class JudgesControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private UserManager<ApplicationUser> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new UserManager<ApplicationUser>(store.Object, null, null, null, null, null, null, null, null);
        }

        private RoleManager<IdentityRole> GetRoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new RoleManager<IdentityRole>(store.Object, null, null, null, null);
        }

        [Fact]
        public async Task Index_ReturnsViewWithJudges()
        {
            var context = GetInMemoryContext();
            context.Judges.Add(new Judge
            {
                JudgeId = 1,
                FullName = "John Doe",
                QualificationLevel = "First",
                ApplicationUser = new ApplicationUser { PhotoUrl = "/images/1.jpg", FullName = "John Doe" }
            });
            await context.SaveChangesAsync();

            var controller = new JudgesController(context, GetUserManagerMock(), GetRoleManagerMock());

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<JudgesController.JudgeViewModel>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_ReturnsNotFoundIfIdIsNull()
        {
            var controller = new JudgesController(GetInMemoryContext(), GetUserManagerMock(), GetRoleManagerMock());
            var result = await controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ValidJudge_AddsJudgeAndRedirects()
        {
            var context = GetInMemoryContext();

            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);

            userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Judge"))
                .ReturnsAsync(IdentityResult.Success);

            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null);
            roleManagerMock.Setup(r => r.RoleExistsAsync("Judge")).ReturnsAsync(true);

            var controller = new JudgesController(context, userManagerMock.Object, roleManagerMock.Object);

            var model = new JudgeCreateViewModel
            {
                Email = "judge@example.com",
                Password = "StrongPass123!",
                FullName = "Judge Joe",
                QualificationLevel = "Senior"
            };

            var result = await controller.Create(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Single(context.Judges);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            var controller = new JudgesController(GetInMemoryContext(), GetUserManagerMock(), GetRoleManagerMock());
            var result = await controller.Edit(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_WithDependencies_ReturnsViewWithError()
        {
            var context = GetInMemoryContext();
            var judge = new Judge
            {
                JudgeId = 1,
                ApplicationUserId = "abc",
                FullName = "Joe",
                QualificationLevel = "Pro",
                Matches = new List<SportSystem2.Models.Match> { new SportSystem2.Models.Match() }
            };
            context.Judges.Add(judge);
            await context.SaveChangesAsync();

            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);

            var controller = new JudgesController(context, userManagerMock.Object, GetRoleManagerMock());

            var result = await controller.DeleteConfirmed(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}
