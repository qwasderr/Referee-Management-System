using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SportSystem2.Controllers;
using SportSystem2.Models;
using Xunit;
namespace Tests;
public class AccountControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public AccountControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _envMock = new Mock<IWebHostEnvironment>();
    }

    private AccountController CreateController()
    {
        return new AccountController(_userManagerMock.Object, _envMock.Object);
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

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
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
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
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
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        _envMock.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

        var user = new ApplicationUser();
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var content = "Fake Image Content";
        var fileName = "test.jpg";

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns<Stream, System.Threading.CancellationToken>((stream, token) => ms.CopyToAsync(stream, token));

        var controller = CreateController();

        var result = await controller.UploadPhoto(fileMock.Object);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Profile", redirectResult.ActionName);
        Assert.NotNull(user.PhotoUrl);
        Assert.Contains("/uploads/", user.PhotoUrl);
    }

    [Fact]
    public async Task UploadPhoto_Post_UpdateFails_ReturnsViewWithModelError()
    {
        _envMock.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

        var user = new ApplicationUser();
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(10);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        var result = await controller.UploadPhoto(fileMock.Object);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(""));
    }
}
