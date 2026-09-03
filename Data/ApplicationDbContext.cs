using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureJwtApi.Models;

namespace SecureJwtApi.Data;

// DbContext that integrates with Identity.
// We pass AppUser as the user type and IdentityRole as the role type.
public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // We can add custom DbSet<T> properties here later (e.g., for Products, Orders).
    // For now, Identity provides all the tables we need.
}