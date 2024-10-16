using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PROG6212POE.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    //user profile - additional features
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    //adding lecturerId variable for the ASP.NET Users table
    public string? LecturerId { get; set; }
}

