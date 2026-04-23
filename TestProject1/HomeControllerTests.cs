using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Project_X_Data.Controllers;
using Project_X_Data.Data;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models;
using Project_X_Data.Models.LogInOut;
using Project_X_Data.Services.Kdf;
using Project_X_Data.Services.MailKit;
using Project_X_Data.Services.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

public class HomeControllerTests : IDisposable
{
    private readonly Mock<ILogger<HomeController>> _mockLogger;
    private readonly Mock<IKdfService> _mockKdfService;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly DataContext _fakeDataContext;
    private readonly Mock<DataAccessor> _mockDataAccessor;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<IEmailSender> _mockEmailSender;

    private readonly HomeController _controller;
    private readonly ICacheEntry _fakeCacheEntry = Mock.Of<ICacheEntry>();

    public HomeControllerTests()
    {
        // 1. НАЛАШТУВАННЯ IN-MEMORY КОНТЕКСТУ
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _fakeDataContext = new DataContext(options);

        // 2. Ініціалізація моків
        _mockLogger = new Mock<ILogger<HomeController>>();
        _mockKdfService = new Mock<IKdfService>();
        _mockStorageService = new Mock<IStorageService>();
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockEmailSender = new Mock<IEmailSender>();

        // 3. НАЛАШТУВАННЯ MOCK ДЛЯ DataAccessor (Виправлено: 2 параметри)
        _mockDataAccessor = new Mock<DataAccessor>(
            _fakeDataContext,           // Param 1: DataContext
            _mockKdfService.Object      // Param 2: IKdfService
        );

        // Налаштування IMemoryCache
        _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                        .Returns(_fakeCacheEntry);

        // Створення контролера
        _controller = new HomeController(
            _mockLogger.Object,
            _mockKdfService.Object,
            _mockStorageService.Object,
            _fakeDataContext,
            _mockDataAccessor.Object,
            _mockMemoryCache.Object,
            _mockEmailSender.Object
        );
    }

    // Очищення In-Memory DB після кожного тесту
    public void Dispose()
    {
        _fakeDataContext.Database.EnsureDeleted();
        _fakeDataContext.Dispose();
    }

    // ====================================================================
    // ТЕСТИ ДЛЯ REGISTRATION
    // ====================================================================

    [Fact]
    public void Registration_ValidDataAndNewUser_SavesToCacheAndRedirects()
    {

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("profile.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);

        var model = new RegistrationViewModel
        {
            Email = "new@example.com",
            Password = "P@ssword123",
            FirstName = "Test",
            LastName = "User",
            ProfileImageUrl = mockFile.Object
        };

        _controller.ModelState.Clear();
        // Налаштування мок-методів (тепер вони віртуальні)
        _mockDataAccessor.Setup(da => da.GetUserByEmail(model.Email)).Returns((User)null);
        _mockDataAccessor.Setup(da => da.SetRole());
        _mockStorageService.Setup(ss => ss.Save(mockFile.Object)).Returns("saved_filename.jpg");

        // ACT
        var result = _controller.Registration(model);

        // ASSERT
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Verification", redirectResult.ActionName);
        _mockStorageService.Verify(ss => ss.Save(mockFile.Object), Times.Once);
        _mockDataAccessor.Verify(da => da.SetRole(), Times.Once);
        _mockMemoryCache.Verify(mc => mc.CreateEntry(model.Email), Times.Once);
        _mockEmailSender.Verify(
            es => es.SendEmail(model.Email, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public void Registration_EmailExists_ReturnsViewWithError()
    {
        // ARRANGE
        var model = new RegistrationViewModel { Email = "exists@example.com" };
        _mockDataAccessor.Setup(da => da.GetUserByEmail(model.Email)).Returns(new User());

        // ACT
        var result = _controller.Registration(model);

        // ASSERT
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains("Email already exists", _controller.ModelState["Email"].Errors[0].ErrorMessage);
    }

    [Fact]
    public void Registration_InvalidModelState_ReturnsViewWithModel()
    {
        // ARRANGE
        var model = new RegistrationViewModel { Email = "invalid@data.com" };
        _controller.ModelState.AddModelError("Email", "The Email field is required.");

        // ACT
        var result = _controller.Registration(model);

        // ASSERT
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        _mockDataAccessor.Verify(da => da.GetUserByEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Registration_NullProfileImage_DoesNotCallStorageService()
    {
        // ARRANGE
        var model = new RegistrationViewModel
        {
            Email = "nofile@example.com",
            Password = "P@ssword123",
            ProfileImageUrl = null
        };
        _controller.ModelState.Clear();
        _mockDataAccessor.Setup(da => da.GetUserByEmail(model.Email)).Returns((User)null);
        _mockDataAccessor.Setup(da => da.SetRole());

        // ACT
        var result = _controller.Registration(model);

        // ASSERT
        Assert.IsType<RedirectToActionResult>(result);
        _mockStorageService.Verify(
            ss => ss.Save(It.IsAny<IFormFile>()),
            Times.Never
        );
        _mockDataAccessor.Verify(da => da.SetRole(), Times.Once);
    }

    // ====================================================================
    // ТЕСТИ ДЛЯ VERIFICATION
    // ====================================================================

    [Fact]
    public void Verification_ValidCode_CreatesUserAndReturnsOk()
    {
        // ARRANGE
        var model = new VerificationModel { Email = "verify@test.com", Code = "12345" };
        var registrationData = new RegistrationSave
        {
            Email = model.Email,
            Code = model.Code,
            Password = "P@ssword123",
            FullName = "Verified User",
            BirthDate = DateTime.Today
        };

        object outRegistrationData = registrationData;
        _mockMemoryCache.Setup(mc => mc.TryGetValue(model.Email, out outRegistrationData))
                         .Returns(true);

        _mockKdfService.Setup(kdf => kdf.GenerateSalt()).Returns("FakeSalt");
        _mockKdfService.Setup(kdf => kdf.Dk(It.IsAny<string>(), It.IsAny<string>())).Returns("FakeDk");

        // ACT
        var result = _controller.Verification(model);

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseData = Assert.IsType<dynamic>(okResult.Value);
        Assert.Equal("Registration successful", (string)responseData.Status);

        Assert.Equal(1, _fakeDataContext.Users.Count());
        Assert.Equal(1, _fakeDataContext.UserAccesses.Count());

        _mockMemoryCache.Verify(mc => mc.Remove(model.Email), Times.Once);
    }

    [Fact]
    public void Verification_InvalidCode_ReturnsBadRequest()
    {
        // ARRANGE
        var model = new VerificationModel { Email = "wrongcode@test.com", Code = "WRONG" };
        var registrationData = new RegistrationSave { Email = model.Email, Code = "12345" };

        object outRegistrationData = registrationData;
        _mockMemoryCache.Setup(mc => mc.TryGetValue(model.Email, out outRegistrationData))
                         .Returns(true);

        // ACT
        var result = _controller.Verification(model);

        // ASSERT
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorObject = Assert.IsType<dynamic>(badRequestResult.Value);
        Assert.Equal("Invalid code", (string)errorObject.message);

        Assert.Equal(0, _fakeDataContext.Users.Count());
    }
}