//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace PROG6212POE.Controllers
//{
//    public class HRController : Controller
//    {
//        //[Authorize(Roles = "HR")]
//        public IActionResult HRIndex()
//        {
//            return View();
//        }
//        //[Authorize(Roles = "HR")]
//        public IActionResult Invoices()
//        {
//            return View();
//        }

//        public async Task<IActionResult> GenerateInvoice()
//        {
//            TempData["message"] = "Invoice generated successfully";
//            return RedirectToAction("Invoices");
//        }

//        //[Authorize(Roles = "HR")]
//        public IActionResult ListLecturers()
//        {
//            return View();
//        }
//        //[Authorize(Roles = "HR")]
//        public IActionResult UpdateLecturerInformation()
//        {
//            return View();
//        }

//        public async Task<IActionResult> UpdateInformation()
//        {
//            TempData["message"] = "User updated successfully";
//            return RedirectToAction("ListLecturers");
//        }
//    }
//}
