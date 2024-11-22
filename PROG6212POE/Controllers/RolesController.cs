using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROG6212POE.Areas.Identity.Data;

namespace PROG6212POE.Controllers
{
    public class RolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //promoting lecturer
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> PromoteLecturer()
        {
            //pulls all users with the lecturer role
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var lecturers = (await _userManager.GetUsersInRoleAsync("Lecturer")).ToList();
            return View(lecturers);
        }

        //generic method that promotes a user to a chosen staff role
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteUser(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);

            //remove role and id
            await _userManager.RemoveFromRoleAsync(user, "Lecturer");
            user.LecturerId = null;
            //add to new role
            await _userManager.AddToRoleAsync(user, newRole);
            //save
            await _userManager.UpdateAsync(user);
            TempData["message"] = $"User {user.UserName} promoted to {newRole} successfully";
            TempData["messageType"] = "success";
            TempData.Keep("message");
            return RedirectToAction("PromoteLecturer");
        }

        //superadmin viewing and creating admin
        //[Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ViewAdmins()
        {
            //gets all users with admin role
            var admins = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();
            return View(admins);
        }

        //ability to demote admin to lecturer
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DemoteAdmin(string userId)
        {
            //form tag in view calls this method
            //gets user
            var user = await _userManager.FindByIdAsync(userId);

            //removing from role
            await _userManager.RemoveFromRoleAsync(user, "Admin");

            //generating lecturerID
            user.LecturerId = $"LID-{Guid.NewGuid().ToString().Substring(0, 4)}";

            //adding to role
            await _userManager.AddToRoleAsync(user, "Lecturer");

            await _userManager.UpdateAsync(user);

            //success messages
            TempData["message"] = $"{user.UserName} demoted successfully";
            TempData["messageType"] = "error";
            TempData.Keep("message");

            return RedirectToAction("ViewAdmins");
        }

        //barebones register page that grants admin role from beginning
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        //post method for create admin
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin(string username, string password, string confirmPassword)
        {
            //controller check for the password and confirm password
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                TempData["message"] = "Passwords to not match";
                TempData["messageType"] = "error";
                TempData.Keep("message");
                return View();
            }
            //making a new application user object
            var adminUser = new ApplicationUser { UserName = username };
            //creating user as shown in register.cshtml.cs
            var result = await _userManager.CreateAsync(adminUser, password);

            //if successful creation of user, do the following
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                //prevents unauthorized login of account
                adminUser.EmailConfirmed = true;
                //toastr
                TempData["message"] = "Admin created successfully";
                TempData["messageType"] = "success";
                TempData.Keep("message");
                return RedirectToAction("ViewAdmins");
            }
        return View();
        }
    }
}
