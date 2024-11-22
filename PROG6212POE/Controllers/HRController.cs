using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PROG6212POE.Areas.Identity.Data;
using System.Net.Mail;
using System.Net;
using PROG6212POE.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using PROG6212POE.ViewModel;

namespace PROG6212POE.Controllers
{
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        //ctor
        public HRController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        //index where all invoiceable claims are shown
        [Authorize(Roles = "HR")]
        public IActionResult InvoicingIndex()
        {
            var claims = _context.Claims
                .Where(c => c.Status == "Approved")
                .ToList();
            return View(claims);
        }

        //generates invoice info and allows hr user to view it before emailing it
        [Authorize(Roles = "HR")]
        [HttpGet]
        public async Task<IActionResult> PreviewInvoice(string lecturerId, int claimId)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimId);
            var lecturer = await _context.Users.FirstOrDefaultAsync(l => l.LecturerId == lecturerId);
            //sets tax rate to zero so its initialized every time its called
            double taxRate = 0;
            int setAmount = 0;

            //if else chain to check total amount
            //tax tables
            if (claim.TotalAmount > 0 && claim.TotalAmount <= 237101) {taxRate = 0.18; setAmount = 0; }
            else if (claim.TotalAmount <= 237102 && claim.TotalAmount >= 370501) { taxRate = 0.26; setAmount = 42678; }
            else if (claim.TotalAmount >= 370502 && claim.TotalAmount <= 512801) { taxRate = 0.31; setAmount = 77362; }
            else if (claim.TotalAmount <= 512802 && claim.TotalAmount <= 673001) { taxRate = 0.36; setAmount = 121475; }
            else if (claim.TotalAmount <= 673002 && claim.TotalAmount <= 857901) { taxRate = 0.39; setAmount = 179147; }
            else if (claim.TotalAmount <= 857902 && claim.TotalAmount <= 1817001) { taxRate = 0.41; setAmount = 251258; }
            else { taxRate = 0.45; setAmount = 644489; }

            //generates tax amount
            double taxAmount = (claim.TotalAmount * taxRate) + setAmount;
            double totalWithDeduction = claim.TotalAmount - taxAmount;

            //invoice view model that displays new info on the preview invoice view
            var invoiceViewModel = new InvoiceViewModel
            {
                LecturerId = lecturer.LecturerId,
                LecturerName = $"{lecturer.FirstName} {lecturer.LastName}",
                LecturerEmail = lecturer.Email,
                ClaimId = claim.ClaimId,
                ClaimPeriod = $"{claim.ClaimsPeriodStart} to {claim.ClaimsPeriodEnd}",
                TotalAmount = claim.TotalAmount,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                SetAmount = setAmount,
                TotalWithDeduction = totalWithDeduction
            };

            return View(invoiceViewModel);
        }

        //emails user their invoice summary
        [Authorize(Roles = "HR")]
        [HttpPost]
        public async Task<IActionResult> EmailInvoice(string lecturerId, int claimId, double taxRate, double setAmount)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimId);
            var lecturer = await _context.Users.FirstOrDefaultAsync(l => l.LecturerId == lecturerId);

            //generate excel worksheet
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add($"{lecturer.UserName} Claim details");

                //Set headers
                worksheet.Cells[1, 1].Value = "Lecturer ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Claim ID";
                worksheet.Cells[1, 4].Value = "Claim Period";
                worksheet.Cells[3, 1].Value = "Total Amount";
                worksheet.Cells[3, 2].Value = "Tax Rate";
                worksheet.Cells[3, 3].Value = "Tax Amount";
                worksheet.Cells[3, 4].Value = "Total with tax deductions";

                //Style headers
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Red);
                }

                //Add data
                worksheet.Cells[2, 1].Value = lecturer.LecturerId;
                worksheet.Cells[2, 2].Value = $"{lecturer.FirstName} {lecturer.LastName}";
                worksheet.Cells[2, 3].Value = claim.ClaimId;
                worksheet.Cells[2, 4].Value = $"{claim.ClaimsPeriodStart} to {claim.ClaimsPeriodEnd}";

                //styling
                using (var range = worksheet.Cells[3, 1, 3, 4])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Red);
                }
                //more data
                worksheet.Cells[4, 1].Value = claim.TotalAmount;
                worksheet.Cells[4, 2].Value = $"{taxRate * 100} %";
                worksheet.Cells[4, 3].Value = $"R{(claim.TotalAmount * taxRate) + setAmount}";
                worksheet.Cells[4, 4].Value = $"R{claim.TotalAmount - (claim.TotalAmount * taxRate) - setAmount}";

                //to make final payment bold
                using (var singleCell = worksheet.Cells[4, 4])
                {
                    singleCell.Style.Font.Bold = true;

                    //border to this cell
                    singleCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    singleCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    singleCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    singleCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                worksheet.Cells.AutoFitColumns();

                //save excel file to memory stream
                using var memoryStream = new MemoryStream();
                package.SaveAs(memoryStream);
                memoryStream.Position = 0;

                //prepare data for emailing
                var email = new Email
                {
                    ToEmail = lecturer.Email,
                    Subject = $"Your claim invoice",
                    Body = $"Good day sir/ma'am\n\nAttached is your invoice for your claim",
                    Attachment = new Attachment(memoryStream, $"Claim_{claimId}_Invoice.xlsx")
                };

                await SendEmailAsync(email);

                claim.InvoiceEmailed = true;

                await _context.SaveChangesAsync();

                TempData["message"] = "Invoice emailed successfully";
                TempData["messageType"] = "success";
                TempData.Keep("message");
                return RedirectToAction("InvoicingIndex");
            }
        }

        //send email service method
        [Authorize(Roles = "HR")]
        private async Task SendEmailAsync(Email email)
        {
            //..not my proudest moment
            string fromEmail = "jerrybutfortesting@gmail.com";
            string fromPassword = "egtzvelddoyzflif";

            //new mail object
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromEmail);
            message.Subject = email.Subject;
            message.To.Add(new MailAddress(email.ToEmail));
            message.Body = email.Body.ToString();
            message.IsBodyHtml = true;

            //attachment
            message.Attachments.Add(email.Attachment);

            //client
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }

        //updating lecturer details
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ListLecturers()
        {
            var lecturers = await _userManager.GetUsersInRoleAsync("Lecturer");

            //view model to display info on the view
            var lecturerViewModels = lecturers.Select(user => new LecturerViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                LecturerId = user.LecturerId
            }).ToList();
            return View(lecturerViewModels);
        }

        //method that populates edit lecturer view
        [Authorize(Roles = "HR")]
        [HttpGet]
        public async Task<IActionResult> EditLecturer(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            //as always
            var lecturerViewModel = new LecturerViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                LecturerId = user.LecturerId
            };
            return View(lecturerViewModel);
        }

        //action the edits lecturer information
        [Authorize(Roles = "HR")]
        [HttpPost]
        public async Task<IActionResult> EditLecturer(LecturerViewModel lecturerViewModel)
        {
            var user = await _userManager.FindByIdAsync(lecturerViewModel.UserId);
            //making it default back to the original value if there there is nothing in the updated one
            user.FirstName = lecturerViewModel.FirstName ?? user.FirstName;
            user.LastName = lecturerViewModel.LastName ?? user.LastName;
            user.Email = lecturerViewModel.Email ?? user.Email;
            user.PhoneNumber = lecturerViewModel.PhoneNumber ?? user.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            //toastr
            TempData["message"] = "Lecturer information updated successfully";
            TempData["messageType"] = "success";
            TempData.Keep("message");
            return RedirectToAction("ListLecturers");
        }
    }
}
