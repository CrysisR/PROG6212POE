using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PROG6212POE.Areas.Identity.Data;
using PROG6212POE.Controllers;
using PROG6212POE.Models;
using System.Security.Claims;

public class ClaimsControllerTest
{
    //variables for controller, db, and mocking db and applicationuser
    private readonly ClaimsController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    // cotr
    public ClaimsControllerTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "PROG6212POE").Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        //fake path for the enviroment for the database
        _mockEnvironment.Setup(env => env.WebRootPath).Returns("C:\\PathThatSeemsToBeFake");

        //mocking user with a helper method
        _userManagerMock = MockUserManager();

        //sending the mocked data to the controller constructor
        _controller = new ClaimsController(_context, _mockEnvironment.Object, _userManagerMock.Object);

        // Set up TempData
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        // Mock HttpContext
        var httpContext = new DefaultHttpContext();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "testLecturerId"),
            new Claim(ClaimTypes.Name, "testRole"),
        }, "mock"));

        //giving the mocked user to the httpcontext
        httpContext.User = user;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Mocking user for the IsAuthentcated and IsInRole
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "LID-0001"),
            new Claim(ClaimTypes.Role, "Lecturer")
        }, "mock"));
    }

    // First Unit Test
    [Fact]
    public async void ClaimsPost_ValidModel_RedirectToLandingPage()
    {
        // Arrange data
        var claims = new Claims
        {
            LecturerId = "LID-0001",
            FirstName = "Jerry",
            LastName = "Naidoo",
            ClaimsPeriodStart = new DateOnly(2024, 10, 16),
            ClaimsPeriodEnd = new DateOnly(2024, 10, 24),
            HoursWorked = 40,
            RatePerHour = 200,
            DescriptionOfWork = "This is a claim",
            SupportingDocuments = new List<IFormFile>(),
            Status = "Pending"
        };

        // performing the submit with the test data
        var result = await _controller.SubmitClaim(claims);

        // Using the inserts to check
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("LandingPage", redirectResult.ActionName);
        Assert.Equal(1, _context.Claims.Count());
    }

    [Fact]
    public async Task ClaimsPost_InvalidModel_ReturnsRedirectToActionCreateClaim()
    {
        //arrange
        var claims = new Claims
        {
            LecturerId = "LID-0001",
            FirstName = null,
            LastName = "",
            ClaimsPeriodStart= new DateOnly(2024,1,1),
            ClaimsPeriodEnd= new DateOnly(2024,1,1),
            HoursWorked = 9999999,
            RatePerHour = 9999999,
            DescriptionOfWork = "Invalid entry",
            SupportingDocuments = new List<IFormFile>(),
            Status = "Invalid"
        };

        _controller.ModelState.AddModelError("FirstName", "Required");
        var result = await _controller.SubmitClaim(claims);

        //assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        //ensures that the model returns
        Assert.Equal("CreateClaim", redirectResult.ActionName);

        Assert.Equal("Error! One or more fields are blank or incorrectly formatted", _controller.TempData["message"]);

        //false model
        Assert.False(_controller.ModelState.IsValid);
    }

    //testing approve claim action
    [Fact]
    public async void ApproveClaim_RedirectToAllClaims()
    {
        //claim data
        var claims = new Claims
        {
            LecturerId = "LID-0001",
            FirstName = "Jerry",
            LastName = "Naidoo",
            ClaimsPeriodStart = new DateOnly(2024, 10, 16),
            ClaimsPeriodEnd = new DateOnly(2024, 10, 24),
            HoursWorked = 40,
            RatePerHour = 200,
            DescriptionOfWork = "This is a claim",
            SupportingDocuments = new List<IFormFile>(),
            Status = "Pending"
        };
        //saving to db to be updated
        _context.Claims.Add(claims);
        _context.SaveChanges();

        //mocking staff user for the approve claims method
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "ProgrammeCoordinatorUser"),
            new Claim(ClaimTypes.Role, "ProgrammeCoordinator")
        }, "mock"));

        //set user in controller context
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        //calling approve method itself 
        var result = await _controller.ApproveClaim(claims.ClaimId);

        //conditions
        var updatedClaim = _context.Claims.Find(claims.ClaimId);
        var redirectedResult = Assert.IsType<RedirectToActionResult>(result);
        //claim check redirect
        Assert.Equal("AllClaims", redirectedResult.ActionName);
        Assert.Equal("Claims", redirectedResult.ControllerName);

        //actual update check
        Assert.Equal("Approved", updatedClaim.Status);
        //toastr checks
        Assert.Equal("Claim approved", _controller.TempData["message"]);
        Assert.Equal("success", _controller.TempData["messageType"]);
    }

    //testing deny claim action
    [Fact]
    public async void DenyClaim_RedirectToAllClaims()
    {
        //claim data
        var claims = new Claims
        {
            LecturerId = "LID-0001",
            FirstName = "Jerry",
            LastName = "Naidoo",
            ClaimsPeriodStart = new DateOnly(2024, 10, 16),
            ClaimsPeriodEnd = new DateOnly(2024, 10, 24),
            HoursWorked = 40,
            RatePerHour = 200,
            DescriptionOfWork = "This is a claim",
            SupportingDocuments = new List<IFormFile>(),
            Status = "Pending"
        };
        //saving to db to be updated
        _context.Claims.Add(claims);
        _context.SaveChanges();

        //mocking staff user for the approve claims method
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "ProgrammeCoordinatorUser"),
            new Claim(ClaimTypes.Role, "ProgrammeCoordinator")
        }, "mock"));

        //set user in controller context
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        //calling deny method itself 
        var result = await _controller.DenyClaim(claims.ClaimId);

        //conditions
        var updatedClaim = _context.Claims.Find(claims.ClaimId);
        var redirectedResult = Assert.IsType<RedirectToActionResult>(result);
        //claim check redirect
        Assert.Equal("AllClaims", redirectedResult.ActionName);
        Assert.Equal("Claims", redirectedResult.ControllerName);

        //actual update check
        Assert.Equal("Denied", updatedClaim.Status);
        //toastr checks
        Assert.Equal("Claim denied", _controller.TempData["message"]);
        Assert.Equal("error", _controller.TempData["messageType"]);
    }

    // Helper Method to Mock UserManager with required dependencies
    private Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        //nullifying the UserManager dependencies as we do not need them for the mock
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        return userManagerMock;
    }
}
