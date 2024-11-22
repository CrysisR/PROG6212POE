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

public class HRControllerTest
{
    //variables for controller, db, and mocking db and applicationuser
    private readonly HRController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    // cotr
    public HRControllerTest()
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
        _controller = new HRController(_context, _mockEnvironment.Object, _userManagerMock.Object);

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
    //test to see that the method correctly calculates tax, the deductions and populates the view model
    [Fact]
    public async Task PreviewInvoice_ReturnsCorrectInvoiceModel()
    {
        //arrange
        // Seed data
        var lecturer = new ApplicationUser
        {
            Id = "1",
            LecturerId = "LID-001",
            FirstName = "Jerry",
            LastName = "Naidoo",
            Email = "jerry@jerry.com"
        };

        var claim = new Claims
        {
            ClaimId = 1,
            FirstName = lecturer.FirstName,
            LastName = lecturer.LastName,
            LecturerId = lecturer.LecturerId,
            ClaimsPeriodStart = new DateOnly(2024, 1, 1),
            ClaimsPeriodEnd = new DateOnly(2024, 1, 15),
            TotalAmount = 500000
        };

        _context.Users.Add(lecturer);
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        //act
        var result = await _controller.PreviewInvoice(lecturer.LecturerId, claim.ClaimId);

        //Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InvoiceViewModel>(viewResult.Model);

        Assert.Equal("LID-001", model.LecturerId);
        Assert.Equal("Jerry Naidoo", model.LecturerName);
        Assert.Equal("jerry@jerry.com", model.LecturerEmail);
        Assert.Equal(1, model.ClaimId);
        Assert.Equal("2024/01/01 to 2024/01/15", model.ClaimPeriod);
        Assert.Equal(500000, model.TotalAmount);

        // Verify tax calculation
        Assert.Equal(0.31, model.TaxRate); // Tax rate for this range
        Assert.Equal(77362, model.SetAmount); // Set amount for this range
        Assert.Equal(232362, model.TaxAmount); // Tax amount = (500,000 * 0.31) + 77,362 = 232,362
        Assert.Equal(267638, model.TotalWithDeduction); // Total deduction = 500,000 - 232,362 = 267638
    }

    //test to check that the editLecturer method works as expected
    [Fact]
    public async Task EditLecturer_UpdateUserDetailsAndRedirects()
    {
        //arrange
        var user = new ApplicationUser
        {
            Id = "1",
            FirstName = "Jerry",
            LastName = "Naidoo",
            Email = "jerry@jerry.com",
            PhoneNumber = "1234567890"
        };

        _userManagerMock.Setup(um => um.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var updatedLecturerViewModel = new LecturerViewModel
        {
            UserId = "1",
            FirstName = "NewName", // Updated first name
            LastName = null, // Should retain original last name
            Email = "jerrybutadmin@jerry.com", // Updated email
            PhoneNumber = null // Should retain original phone number
        };

        //Act
        var result = await _controller.EditLecturer(updatedLecturerViewModel);

        //Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ListLecturers", redirectResult.ActionName);

        // Check if user details were updated correctly
        Assert.Equal("NewName", user.FirstName); // Updated first name
        Assert.Equal("Naidoo", user.LastName); // Original last name retained
        Assert.Equal("jerrybutadmin@jerry.com", user.Email); // Updated email
        Assert.Equal("1234567890", user.PhoneNumber); // Original phone number retained
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
