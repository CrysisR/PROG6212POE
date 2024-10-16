using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//using PROG6212POE.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();

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

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    app.MapRazorPages());

//need to do this
// setup a scope -- for the users roles
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

//    // allocate 1 - admin 2 - lecturer
//    var roles = new[] {"Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };
//    foreach (var role in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(role))
//            await roleManager.CreateAsync(new IdentityRole(role));
//        //allocate username and password
//        string email = "admin@admin.com";
//        string password = "Adm!n1234";

//        if (await userManager.FindByEmailAsync(email) == null)
//        {
//            var user = new ApplicationUser();
//            user.Email = email;
//            user.UserName = email;
//            user.EmailConfirmed = true;

//            await userManager.CreateAsync(user, password);
//            await userManager.AddToRoleAsync(user, "Admin");
//        }
//    }
//}
app.Run();
