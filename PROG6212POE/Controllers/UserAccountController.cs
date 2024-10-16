using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROG6212POE.Areas.Identity.Data;

namespace PROG6212POE.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserAccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }
        public IActionResult UserAccountDetails()
        {
            return View();
        }
        //part 3
        //public IActionResult UpdateLecturerInformation()
        //{
        //    return View();
        //}

        //custom logout since identity logout was not working
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("LoginOrRegister", "Home");
        }
    }
}
