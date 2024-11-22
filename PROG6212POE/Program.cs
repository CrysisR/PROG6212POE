using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROG6212POE.Areas.Identity.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
//adding email service from configuration
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var app = builder.Build();
//adding services to container

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=LoginOrRegister}/{id?}",
    app.MapRazorPages());

//need to do this
// setup a scope -- for the users roles
// part 2
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // allocate 1 - admin 2 - lecturer
    var roles = new[] { "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR", "SuperAdmin", "Admin" };
    //program coordinator
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        //allocate username and password
        string email = "pc@test.com";
        string password = "Adm!n1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser();
            user.FirstName = "Programme";
            user.LastName = "Coordinator";
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.LecturerId = null;

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "ProgrammeCoordinator");
        }
    }
    //academic manager
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        //allocate username and password
        string email = "am@test.com";
        string password = "Adm!n1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser();
            user.FirstName = "Academic";
            user.LastName = "Manager";
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.LecturerId = null;

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "AcademicManager");
        }
    }
    //Lecturer
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        //allocate username and password
        string email = "lecturer@test.com";
        string password = "Adm!n1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser();
            user.FirstName = "Jerry";
            user.LastName = "Naidoo";
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.LecturerId = "LID-0001";

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "Lecturer");
        }
    }
    //HR
    //part poe
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        //allocate username and password
        string email = "hr@test.com";
        string password = "Adm!n1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser();
            user.FirstName = "Human";
            user.LastName = "Resources";
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.LecturerId = null;

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "HR");
        }
    }
    //superadmin - makes admins
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        //allocate username and password
        string email = "superadmin@test.com";
        string password = "Adm!n1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser();
            user.FirstName = "Super";
            user.LastName = "Admin";
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.LecturerId = null;

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
    }
    //GeneralAdmin - makes other roles
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
        //allocate username and password
        string email = "admin@test.com";
        string password = "Adm!n1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser();
            user.FirstName = "General";
            user.LastName = "Admin";
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.LecturerId = null;

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
app.Run();
