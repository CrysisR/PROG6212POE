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
using PROG6212POE.ViewModel;

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
        Assert.Equal("ViewClaimDetails", redirectResult.ActionName);
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

    //testing approveMatchingClaims method
    [Fact]
    public async Task ApproveMatchingClaims_ChangesToApproveAndRedirects()
    {
        //Seed data
        var claim1 = new Claims
        {
            ClaimId = 1,
            LecturerId = "LID-0001",
            FirstName = "Jerry",
            LastName = "Naidoo",
            ClaimsPeriodStart = new DateOnly(2024, 1, 1),
            ClaimsPeriodEnd = new DateOnly(2024, 1, 15),
            RatePerHour = 5000,
            HoursWorked = 1,
            TotalAmount = 5000,
            DescriptionOfWork = "This is a claim",
            Status = "Pending",
            SupportingDocumentFiles = new List<PROG6212POE.Models.File>()
        };

        var claim2 = new Claims
        {
            ClaimId = 2,
            LecturerId = "LID-0002",
            FirstName = "Jerry",
            LastName = "Coffee",
            ClaimsPeriodStart = new DateOnly(2024, 2, 1),
            ClaimsPeriodEnd = new DateOnly(2024, 2, 15),
            RatePerHour = 7000,
            HoursWorked = 1,
            TotalAmount = 7000,
            DescriptionOfWork = "This is another claim",
            Status = "Pending",
            SupportingDocumentFiles = new List<PROG6212POE.Models.File>()
        };

        var claim3 = new Claims
        {
            ClaimId = 3,
            LecturerId = "LID-0003",
            FirstName = "Jerry",
            LastName = "Admin",
            ClaimsPeriodStart = new DateOnly(2024, 3, 1),
            ClaimsPeriodEnd = new DateOnly(2024, 3, 15),
            RatePerHour = 10000,
            HoursWorked = 1,
            TotalAmount = 10000,
            DescriptionOfWork = null,
            Status = "Pending",
            SupportingDocumentFiles = new List<PROG6212POE.Models.File>()
        };

        _context.Claims.AddRange(claim1, claim2, claim3);
        await _context.SaveChangesAsync();

        var crieria = new CriteriaViewModel
        {
            AllowedStartDate = new DateOnly(2024, 1, 1),
            AllowedEndDate = new DateOnly(2024, 12, 31),
            TotalAmount = 8000,
            RequireDescription = true,
            MinimumDocuments = 0
        };

        //user
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

        //Act
        var result = await _controller.ApproveMatchingClaims(crieria);

        //Assert
        var approvedClaims = _context.Claims.Where(c => c.Status == "Approved").ToList();
        //Only 2 claims should match, claim 1 and claim 2
        Assert.Equal(2, approvedClaims.Count);
        Assert.All(approvedClaims, c => Assert.Equal("ProgrammeCoordinatorUser", c.DecidedBy));

        var redirectedResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AllClaims", redirectedResult.ActionName);
    }

    //Testing for no claims that match
    [Fact]
    public async Task ApproveMatchingClaims_NoMatchingClaimsFound_RedirectWithError()
    {
        //arrange
        //seed data
        var claim1 = new Claims
        {
            ClaimId = 1,
            LecturerId = "LID-0001",
            FirstName = "Jerry",
            LastName = "Naidoo",
            ClaimsPeriodStart = new DateOnly(2024, 1, 1),
            ClaimsPeriodEnd = new DateOnly(2024, 1, 15),
            TotalAmount = 5000,
            DescriptionOfWork = "This is a claim",
            Status = "Pending",
            SupportingDocumentFiles = new List<PROG6212POE.Models.File>()
        };

        _context.Claims.Add(claim1);
        await _context.SaveChangesAsync();

        var criteria = new CriteriaViewModel
        {
            AllowedStartDate = new DateOnly(2024, 2, 1),
            AllowedEndDate = new DateOnly(2024, 12, 31),
            TotalAmount = 4000,
            RequireDescription = true,
            MinimumDocuments = 1
        };

        //user
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

        //act
        var result = await _controller.ApproveMatchingClaims(criteria);

        //Assert
        var redirectedResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("SetClaimCriteria", redirectedResult.ActionName);

        var tempDataMessage = _controller.TempData["message"]?.ToString();
        Assert.Contains("No claims found matching those criteria", tempDataMessage);

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
