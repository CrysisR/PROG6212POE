using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PROG6212POE.Areas.Identity.Data;
using PROG6212POE.Models;
using System.Security.Claims;

namespace PROG6212POE.Controllers
{
    public class ClaimsController : Controller
    {
        //variables for db and hosting
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        //ctor
        public ClaimsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        //http get
        [HttpGet]
        public async Task<IActionResult> CreateClaim()
        {
            //custom role based auth becuase Authorize(Roles = "Lecturer") didnt work
            if (User.Identity.IsAuthenticated && User.IsInRole("Lecturer"))
            {
                //gets the current user that is logged in
                var user = await _userManager.GetUserAsync(User);

                //writing the following variables to the model so that in the view, they are
                //automatically pulled and displayed in the text field
                var model = new Claims
                {
                    LecturerId = user.LecturerId,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return View(model);
            }
            //if the user isnt authenticated
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //http post
        [HttpPost]
        [RequestSizeLimit(5000000)] // Limit to 5 MB
        public async Task<IActionResult> SubmitClaim(Claims claims)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Lecturer"))
            {
                //what extensions are allowed for upload
                //doing this before database actions occur so that there is no invalid entries
                var allowedExtensions = new[] { ".pdf", ".xlsx", ".docx", ".pptx", ".png", ".jpg" };

                //since there are multiple files, need a foreach loop
                foreach (var formFile in claims.SupportingDocuments)
                {
                    //lowercasing the extension and checking it against the array
                    var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
                    //fail for invalid file type
                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Invalid file type");
                        TempData["message"] = "Invalid file type";
                        TempData["messageType"] = "error";
                        //the .keep method ensures that the message is kept through redirects
                        TempData.Keep("message");
                        return View("CreateClaim");
                    }
                    //fail for too big size file
                    if(formFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "File too large");
                        TempData["message"] = "File exceeds 5mbs";
                        TempData["messageType"] = "error";
                        TempData.Keep("message");
                        return View("CreateClaim");
                    }
                }
                if (ModelState.IsValid)
                {
                    //do a calculation
                    //draw a toaster (n.b Not the actual kitchen applicance, more like the notification that
                    //pops up when an action is done, which has a smooth animation and a timer line underneath
                    //it that goes down and when it reaches zero it goes away in a nice animation.
                    //This shouldnt be my job, but the job of the css team but for some reason im the judge, jury
                    //and executioner for this entire application, so it is up to me to do this toaster thingy
                    //btw, why is it called a toaster, all i think about is delious toast that i wanna eat atm,
                    //but nope, it refers to a NOTIFICATION on a WEBSITE, like wtf, be clear.)

                    claims.TotalAmount = claims.HoursWorked * claims.RatePerHour;
                    //write the database initially to get claim id
                    _context.Claims.Add(claims);
                    await _context.SaveChangesAsync();


                    // Create a folder for the specific claim using LecturerId and ClaimId
                    string claimFolder = Path.Combine("wwwroot", "documents", claims.LecturerId, claims.ClaimId.ToString());

                    //create directory
                    if (!Directory.Exists(claimFolder))
                    {
                        Directory.CreateDirectory(claimFolder);
                    }

                    // Upload file logic
                    if (claims.SupportingDocuments != null && claims.SupportingDocuments.Count > 0)
                    {
                        var filePaths = new List<string>();
                        foreach (var formFile in claims.SupportingDocuments)
                        {
                            if (formFile.Length > 0)
                            {
                                //get file name and extension
                                var fileName = Path.GetFileNameWithoutExtension(formFile.FileName);
                                var extension = Path.GetExtension(formFile.FileName);
                                var path = Path.Combine(claimFolder, fileName);

                                //saving path to variable to add to claims variable
                                filePaths.Add($"/documents/{claims.LecturerId}/{claims.ClaimId}/{fileName}");
                            }
                        }
                        // Save paths as a string
                        claims.SupportingDocumentsPaths = string.Join(";", filePaths);
                    }
                    //part 2
                    _context.Claims.Update(claims);
                    await _context.SaveChangesAsync();
                    //Toastr
                    TempData["message"] = "Claim submitted successfully";
                    TempData["messageType"] = "success";
                    TempData.Keep("message");
                    return RedirectToAction("LandingPage", "Home");
                }
                else
                {
                    //Toastr
                    TempData["message"] = "Error! One or more fields are blank or incorrectly formatted";
                    TempData["messageType"] = "error";
                    TempData.Keep("message");
                    return RedirectToAction("CreateClaim");
                }
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //view for submitting claim, doesnt show up since we redirect on every code path
        public IActionResult SubmitClaim()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Lecturer"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //see claims made by specific lecturer
        public async Task<IActionResult> ListLecturerClaims()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Lecturer"))
            {
                //This retrieves the current user's Id, couldnt use the normal method of getting UserId
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                //Gets the id of the lecturer currently logged in using LINQ
                var lecturerId = await _context.Users
                                               .Where(u => u.Id == userId)
                                               .Select(u => u.LecturerId)
                                               .FirstOrDefaultAsync();

                //get the claim where the stored lecturerId matches the LecturerId of the currently logged in user
                var claims = await _context.Claims
                    .Where(c => c.LecturerId == lecturerId)
                    .ToListAsync();
                return View(claims);
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //details page of calim
        public async Task<IActionResult> ViewClaimDetails(int id)
        {
            //all users, both Lecturer, PC and AM use the same method to view the claim information
            if (User.Identity.IsAuthenticated && User.IsInRole("Lecturer") || User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"))
            {
                var claim = await _context.Claims.FindAsync(id);

                if (claim == null)
                {
                    return NotFound();
                }

                return View(claim);
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //did not work
        //[Authorize(Roles = "ProgrammeCoordinator")]
        //[Authorize(Roles = "AcademicManager")]
        //sort status is pulled from the claims table to use in the method
        public IActionResult AllClaims(string sortStatus)
        {
            //staff roles only access the view, hence the 2 roles
            if (User.Identity.IsAuthenticated && User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"))
            {
                //gets claims
                var claims = _context.Claims.ToList();
                //sorts claims based on status
                if (!string.IsNullOrEmpty(sortStatus))
                {
                    switch (sortStatus.ToLower())
                    {
                        case "approved":
                            claims = claims.Where(c => c.Status == "Approved").ToList();
                            break;
                        case "denied":
                            claims = claims.Where(c => c.Status == "Denied").ToList();
                            break;
                        case "pending":
                            claims = claims.Where(c => c.Status == "Pending").ToList();
                            break;
                    }
                }
                return View(claims);
            }
            return RedirectToAction("NotAllowed", "Home");
        }

        //claim approval
        //part 3 will figure out a way to email a user after their claim has been accepted or rejected
        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"))
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim != null)
                {
                    //changing claim and toastr
                    claim.Status = "Approved";
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Claim approved";
                    TempData["messageType"] = "success";
                    TempData.Keep("message");
                }
                return RedirectToAction("AllClaims", "Claims");
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }

        //same logic as approve
        [HttpPost]
        public async Task<IActionResult> DenyClaim(int id)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"))
            {
                var claims = await _context.Claims.FindAsync(id);
                if (claims != null)
                {
                    claims.Status = "Denied";
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Claim denied";
                    TempData["messageType"] = "error";
                    TempData.Keep("message"); ;
                }
                return RedirectToAction("AllClaims", "Claims");
            }
            else
            {
                return RedirectToAction("NotAllowed", "Home");
            }
        }
    }
}
