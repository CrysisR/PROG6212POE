using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using PROG6212POE.Areas.Identity.Data;
using PROG6212POE.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

public class RolesControllerTest
{
    private readonly RolesController _controller;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;

    public RolesControllerTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "PROG6212POE").Options;

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

        _controller = new RolesController(_userManagerMock.Object, _roleManagerMock.Object);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    //first unit test
    //test if promote lecturer controller actions works
    [Fact]
    public async Task PromoteLecturer_PromoteToAcademicManager_RedirectToActionPromoteLecturer()
    {
        //arrange
        var testLecturer = new ApplicationUser
        {
            Id = "testLecturerId",
            UserName = "testlecturer@gmail.com",
            LecturerId = "LID-0001"
        };

        //mocking userManagement methods
        _userManagerMock.Setup(um => um.FindByIdAsync("testLecturerId")).ReturnsAsync(testLecturer);
        _userManagerMock.Setup(um => um.RemoveFromRoleAsync(testLecturer, "Lecturer")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRoleAsync(testLecturer, "AcademicManager")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.UpdateAsync(testLecturer)).ReturnsAsync(IdentityResult.Success);

        //mocking httpcontext as a admin
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "adminId"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = adminUser }
        };

        //act 
        var result = await _controller.PromoteUser("testLecturerId", "AcademicManager");

        //Asserts
        _userManagerMock.Verify(um => um.FindByIdAsync("testLecturerId"), Times.Once);
        _userManagerMock.Verify(um => um.RemoveFromRoleAsync(testLecturer, "Lecturer"), Times.Once);
        _userManagerMock.Verify(um => um.AddToRoleAsync(testLecturer, "AcademicManager"), Times.Once);
        _userManagerMock.Verify(um => um.UpdateAsync(testLecturer), Times.Once);

        //check if lecturer id became null
        Assert.Equal(null, testLecturer.LecturerId);

        //tempdata check for toastr
        Assert.Equal($"User {testLecturer.UserName} promoted to AcademicManager successfully", _controller.TempData["message"]);
        Assert.Equal("success", _controller.TempData["messageType"]);

        //test redirected to promote lecturer page
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.PromoteLecturer), redirectResult.ActionName);
    }
    //second unit test
    //check if demote admin logic successfully works
    [Fact]
    public async Task DemoteAdmin_RedirectToActionViewAdmins()
    {
        //arrange model admin user
        var testAdmin = new ApplicationUser
        {
            Id = "testAdminId",
            UserName = "testadmin@gmail.com",
        };

        //mocking usermanagement methods
        _userManagerMock.Setup(um => um.FindByIdAsync("testAdminId")).ReturnsAsync(testAdmin);
        _userManagerMock.Setup(um => um.RemoveFromRoleAsync(testAdmin, "Admin")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRoleAsync(testAdmin, "Lecturer")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.UpdateAsync(testAdmin)).ReturnsAsync(IdentityResult.Success);

        //mocking httpcontext as a superadmin
        var superAdminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "superAdminId"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        }, "mock"));

        //adding to controller context
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = superAdminUser }
        };

        //act
        var result = await _controller.DemoteAdmin(testAdmin.Id);

        //assert
        _userManagerMock.Verify(um => um.FindByIdAsync("testAdminId"), Times.Once);
        _userManagerMock.Verify(um => um.RemoveFromRoleAsync(testAdmin, "Admin"), Times.Once);
        _userManagerMock.Verify(um => um.AddToRoleAsync(testAdmin, "Lecturer"), Times.Once);
        _userManagerMock.Verify(um => um.UpdateAsync(testAdmin), Times.Once);

        //check if lecturerId not null
        Assert.NotNull(testAdmin.LecturerId);

        //tempdata for toastrs
        Assert.Equal($"{testAdmin.UserName} demoted successfully", _controller.TempData["message"]);
        Assert.Equal("error", _controller.TempData["messageType"]);

        //test redirected to view admins page
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.ViewAdmins), redirectResult.ActionName);
    }

    //test 3
    //Checks whether the create admin controller action successfully creates an admin
    [Fact]
    public async Task CreateAdmin_ValidAdminDetails_RedirectToActionViewAdmins()
    {
        //arrange
        var username = "newadmin@test.com";
        var password = "Adm!n1234";
        var confirmPassword = "Adm!n1234";

        //mock admin user
        var adminUser = new ApplicationUser
        {
            UserName = username,
        };

        //mock user management
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRoleAsync(adminUser, "Admin")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.UpdateAsync(adminUser)).ReturnsAsync(IdentityResult.Success);

        //mocking httpcontext as superadmin
        var superAdminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "superAdminId"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = superAdminUser }
        };

        //act
        var result = await _controller.CreateAdmin(username, password, confirmPassword);

        //assert
        _userManagerMock.Verify(um => um.CreateAsync(It.Is<ApplicationUser>(u => u.UserName == username), password), Times.Once);
        _userManagerMock.Verify(um => um.AddToRoleAsync(It.Is<ApplicationUser>(u => u.UserName == username), "Admin"), Times.Once);

        //tempdata for toastrs
        Assert.Equal("Admin created successfully", _controller.TempData["message"]);
        Assert.Equal("success", _controller.TempData["messageType"]);

        //redirect test
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.ViewAdmins), redirectResult.ActionName);
    }
}
