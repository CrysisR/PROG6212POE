using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PROG6212POE.Areas.Identity.Data;
using PROG6212POE.Models;
using PROG6212POE.ViewModel;
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
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public async Task<IActionResult> CreateClaim()
        {
            //custom role based auth becuase Authorize(Roles = "Lecturer") didnt work
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

        //http post
        [Authorize(Roles = "Lecturer")]
        [HttpPost]
        [RequestSizeLimit(5000000)] // Limit to 5 MB
        public async Task<IActionResult> SubmitClaim(Claims claims)
        {
            //what extensions are allowed for upload
            //doing this before database actions occur so that there is no invalid entries
            var allowedExtensions = new[] { ".pdf", ".xlsx", ".docx", ".pptx", ".png", ".jpg" };

            //since there are multiple files, need a foreach loop
            if (claims.SupportingDocuments != null)
            {
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
                    if (formFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "File too large");
                        TempData["message"] = "File exceeds 5mbs";
                        TempData["messageType"] = "error";
                        TempData.Keep("message");
                        return View("CreateClaim");
                    }
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

                //upload file logic
                var uploadedFileNames = new List<string>();
                if (claims.SupportingDocuments != null)
                {
                    foreach (var uploadedFile in claims.SupportingDocuments)
                    {
                        using var memoryStream = new MemoryStream();
                        await uploadedFile.CopyToAsync(memoryStream);

                        var originalFileName = Path.GetFileNameWithoutExtension(uploadedFile.FileName);
                        var fileExtension = Path.GetExtension(uploadedFile.FileName);

                        var guid = Guid.NewGuid().ToString("N").Substring(0, 8);

                        var newFileName = $"{originalFileName}_{guid}{fileExtension}";
                        var newFile = new Models.File
                        {
                            FileName = newFileName,
                            FileLength = uploadedFile.Length,
                            FileData = memoryStream.ToArray(),
                            DateUploaded = DateTime.UtcNow,
                            Claim = claims,
                            ClaimId = claims.ClaimId
                        };

                        //save file to database
                        _context.Files.Add(newFile);
                        await _context.SaveChangesAsync();

                        uploadedFileNames.Add(newFileName);

                        //many to one relationship
                        claims.SupportingDocumentFiles.Add(newFile);
                    }
                    //saving document names
                    claims.SupportingDocumentFileNames = string.Join(";", uploadedFileNames);
                }
                //part 2
                _context.Claims.Update(claims);
                await _context.SaveChangesAsync();
                //Toastr
                TempData["message"] = "Claim submitted successfully";
                TempData["messageType"] = "success";
                TempData.Keep("message");
                return RedirectToAction("ViewClaimDetails", new { id = claims.ClaimId});
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

        //view for submitting claim, doesnt show up since we redirect on every code path
        [Authorize(Roles = "Lecturer")]
        public IActionResult SubmitClaim()
        {
            return View();
        }

        //see claims made by specific lecturer
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> ListLecturerClaims()
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
                .Include(c => c.SupportingDocumentFiles)
                .ToListAsync();
            return View(claims);
        }

        //cancel a claim (remove it from the database)
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> CancelClaim(int claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.SupportingDocumentFiles)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            TempData["message"] = "Claim request cancelled successfully";
            TempData["messageType"] = "success";
            TempData.Keep("message");
            return RedirectToAction("ListLecturerClaims");

        }

        //details page of calim
        [Authorize(Roles = "Lecturer, ProgrammeCoordinator, AcademicManager, HR")]
        public async Task<IActionResult> ViewClaimDetails(int id)
        {
            //all users, both Lecturer, PC and AM use the same method to view the claim information
            var claim = await _context.Claims
                .Include(c => c.SupportingDocumentFiles)
                .FirstOrDefaultAsync(p => p.ClaimId == id);

            return View(claim);
        }

        //to view supporting documents in the browser
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            // Retrieve the file from the database
            var file = await _context.Files.FindAsync(fileId);

            // Get MIME type for the file
            string mimeType = GetFileType(file.FileName);
            string fileName = file.FileName;

            // Return the file as a downloadable content
            return File(file.FileData, mimeType, fileName);
        }

        //gets file type for downloading
        private string GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName.ToLowerInvariant());
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".png" => "image/png",
                ".jpg" or "jpeg" => "image/jpeg",
                _ => "application/octet-stream" // default
            };
        }

        //sort status is pulled from the claims table to use in the method
        [Authorize(Roles = "ProgrammeCoordinator, AcademicManager")]
        public IActionResult AllClaims(string sortStatus)
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

        //claim criteria for auto approval
        [HttpGet]
        [Authorize(Roles = "ProgrammeCoordinator, AcademicManager")]
        public IActionResult SetClaimCriteria()
        {
            return View();
        }

        //approve claims that match the claim criteria
        [HttpPost]
        [Authorize(Roles = "ProgrammeCoordinator, AcademicManager")]
        public async Task<IActionResult> ApproveMatchingClaims(CriteriaViewModel criteria)
        {
            //Fetch claims from db
            var claims = await _context.Claims
                .Include(c => c.SupportingDocumentFiles)
                .Where(c => 
                    c.ClaimsPeriodStart >= criteria.AllowedStartDate &&
                    c.ClaimsPeriodEnd <= criteria.AllowedEndDate &&
                    c.TotalAmount <= criteria.TotalAmount &&
                    (!criteria.RequireDescription || !string.IsNullOrWhiteSpace(c.DescriptionOfWork)) &&
                    c.SupportingDocumentFiles.Count >= criteria.MinimumDocuments)
                .ToListAsync();

            if (!claims.Any())
            {
                TempData["message"] = $"Error! No claims found matching those criteria";
                TempData["messageType"] = "error";
                TempData.Keep("message");
                return RedirectToAction("SetClaimCriteria", criteria);
            }

            foreach(var claim in claims)
            {
                claim.Status = "Approved";
                claim.DecidedBy = User.Identity.Name;
            }

            await _context.SaveChangesAsync();
            //Toastr
            TempData["message"] = $"Claims have been approved";
            TempData["messageType"] = "success";
            TempData.Keep("message");
            return RedirectToAction("AllClaims");
        }

        //claim approval
        [HttpPost]
        [Authorize(Roles = "ProgrammeCoordinator, AcademicManager")]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                //changing claim and toastr
                claim.Status = "Approved";
                claim.DecidedBy = User.Identity.Name;
                await _context.SaveChangesAsync();
                TempData["message"] = "Claim approved";
                TempData["messageType"] = "success";
                TempData.Keep("message");
            }
            return RedirectToAction("AllClaims", "Claims");
        }

        //same logic as approve
        [HttpPost]
        [Authorize(Roles = "ProgrammeCoordinator, AcademicManager")]
        public async Task<IActionResult> DenyClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Denied";
                claim.DecidedBy = User.Identity.Name;
                await _context.SaveChangesAsync();
                TempData["message"] = "Claim denied";
                TempData["messageType"] = "error";
                TempData.Keep("message"); ;
            }
            return RedirectToAction("AllClaims", "Claims");
        }
    }
}
