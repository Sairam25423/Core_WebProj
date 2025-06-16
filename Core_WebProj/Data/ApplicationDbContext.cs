using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core_WebProj.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Core_WebProj.Models.RegisterViewModel> RegisterViewModels { get; set; } = null!;
        public DbSet<Core_WebProj.Models.ApplicationUser> ApplicationUsers { get; set; } = null!;
    }
}
