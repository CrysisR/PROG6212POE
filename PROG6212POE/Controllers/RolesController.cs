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
        //customer auth
        //neither worked
        //[Authorize(Policy = "AdminOnly")]
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> PromoteLecturer()
        {
            //pulls all users with the lecturer role
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                var lecturers = (await _userManager.GetUsersInRoleAsync("Lecturer")).ToList();
                return View(lecturers);
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //generic method that promotes a user to a chosen staff role
        [HttpPost]
        public async Task<IActionResult> PromoteUser(string userId, string newRole)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
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
                return RedirectToAction(nameof(PromoteLecturer));
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //superadmin viewing and creating admin
        //[Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> ViewAdmins()
        {
            //gets all users with admin role
            if (User.Identity.IsAuthenticated && User.IsInRole("SuperAdmin"))
            {
                var admins = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();
                return View(admins);
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //ability to demote admin to lecturer
        [HttpPost]
        public async Task<IActionResult> DemoteAdmin(string userId)
        {
            //form tag in view calls this method
            if (User.Identity.IsAuthenticated && User.IsInRole("SuperAdmin"))
            {
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

                return RedirectToAction(nameof(ViewAdmins));
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //[Authorize(Roles = "SuperAdmin")]
        //barebones register page that grants admin role from beginning
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("SuperAdmin"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //[Authorize(Roles = "SuperAdmin")]
        //post method for create admin
        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string username, string password, string confirmPassword)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("SuperAdmin"))
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
                    //part 3 will have the email confirmed by the user
                    adminUser.EmailConfirmed = true;
                    //toastr
                    TempData["message"] = "Admin created successfully";
                    TempData["messageType"] = "success";
                    TempData.Keep("message");
                    return RedirectToAction(nameof(ViewAdmins));
                }
                return View();
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }
    }
}
