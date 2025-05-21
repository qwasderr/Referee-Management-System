using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SportSystem2.Models;
using SportSystem2.Services;
using System.Security.Claims;

namespace Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IImageService> _imageServiceMock;

        public AccountControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            _imageServiceMock = new Mock<IImageService>();
        }

        private AccountController CreateController()
        {
            return new AccountController(_userManagerMock.Object, _imageServiceMock.Object);
        }

        [Fact]
        public async Task UploadPhoto_Post_NoFile_ReturnsViewWithModelError()
        {
            var controller = CreateController();

            var result = await controller.UploadPhoto(null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task UploadPhoto_Post_InvalidUser_ReturnsNotFound()
        {
            var controller = CreateController();

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var result = await controller.UploadPhoto(fileMock.Object);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("application/pdf")]
        [InlineData("text/plain")]
        [InlineData("image/bmp")]
        public async Task UploadPhoto_Post_InvalidContentType_ReturnsViewWithModelError(string contentType)
        {
            var controller = CreateController();

            var user = new ApplicationUser();
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10);
            fileMock.Setup(f => f.ContentType).Returns(contentType);

            var result = await controller.UploadPhoto(fileMock.Object);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task UploadPhoto_Post_ValidFile_UpdatesUserAndRedirects()
        {
            var user = new ApplicationUser();
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") },
                    "mock"));

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            _imageServiceMock
                .Setup(s => s.SaveImageAsync(It.IsAny<IFormFile>(), "users"))
                .ReturnsAsync("/images/users/test.jpg");

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);

            var controller = CreateController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var result = await controller.UploadPhoto(fileMock.Object);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Profile", redirectResult.ActionName);
            Assert.Equal("/images/users/test.jpg", user.PhotoUrl);

            _imageServiceMock.Verify(s => s.SaveImageAsync(fileMock.Object, "users"), Times.Once);

        }

        [Fact]
        public async Task UploadPhoto_Post_UpdateFails_ReturnsViewWithModelError()
        {
            var user = new ApplicationUser();
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            _imageServiceMock
                .Setup(s => s.SaveImageAsync(It.IsAny<IFormFile>(), "profile"))
                .ReturnsAsync("/images/profile/test.jpg");

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);

            var controller = CreateController();

            var result = await controller.UploadPhoto(fileMock.Object);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));
        }
    }
}
