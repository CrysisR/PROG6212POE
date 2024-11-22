using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROG6212POE.Areas.Identity.Data;
using PROG6212POE.Models;

namespace PROG6212POE.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //To add your claims table into the database
    public DbSet<Claims> Claims { get; set; }
    public DbSet<Models.File> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //allows multiple files to be associated with 1 claim
        builder.Entity<Claims>()
            .HasMany(p => p.SupportingDocumentFiles)
            .WithOne(f => f.Claim)
            .HasForeignKey(f => f.ClaimId);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
