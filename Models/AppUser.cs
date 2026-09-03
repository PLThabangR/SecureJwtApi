using Microsoft.AspNetCore.Identity;

namespace SecureJwtApi.Models
{
    // Custom user class that extends IdentityUser.
    // This allows us to add custom properties later (e.g., FirstName, LastName).
    public class AppUser: IdentityUser
    {
    }
}
