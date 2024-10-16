using Microsoft.AspNetCore.Mvc;
using PROG6212POE.Areas.Identity.Data;
using PROG6212POE.Models;
using System.Diagnostics;

namespace PROG6212POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        //when user logs in
        public IActionResult LandingPage()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }
        //when a user first starts the application
        public IActionResult LoginOrRegister()
        {
            //checks if they are logged in
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            //then checks if they are staff
            else
            {
                if (User.Identity.IsAuthenticated && User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"))
                {
                    return RedirectToAction("StaffLandingPage");
                }
            //if both fail, they are either lecturer or admin
            return RedirectToAction("LandingPage");
            }
        }

        //landing page for staff
        public IActionResult StaffLandingPage()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"))
            {
                //get pending claims for the carousel item in the landing page
                var pendingClaims = _context.Claims
                    .Where(c => c.Status == "Pending").ToList();
                return View(pendingClaims);
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        public IActionResult NotAllowed()
        {
            return View();
        }
    }
}
