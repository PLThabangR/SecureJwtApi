using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureJwtApi.DTOs.Admin;
using SecureJwtApi.Models;
using System.Net;

namespace SecureJwtApi.Controllers;

[Route("api/[controller]")]
[ApiController]
//The key piece – the authorization middleware checks if the authenticated user has a claim of type ClaimTypes.Role with value "Admin". If not, it returns a 403 Forbidden response.
[Authorize(Roles = "Admin")] // Only users with the "Admin" role can access any action in this controller
public class AdminController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

   //Injected so we can optionally fetch user details or count users.The endpoint could be simpler(just return a message), but we include it to show that we can still access the current user's data.
    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: api/admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        // Retrieve the current user (we already know they are authenticated and have the Admin role)
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User ID not found in token" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { Message = "User not found" });
        }
        //A simple database call to show we can aggregate data. In a real scenario, you might fetch real‑time stats.
        // (Optional) We could count total registered users – but that's just a demo.
        var totalUsers = _userManager.Users.Count();

        // Build the dashboard response
        var response = new DashboardResponse
        {
            Message = $"Welcome, Admin {user.Email}! You have access to the admin dashboard.",
            ServerTime = DateTime.UtcNow,
            ActiveUsersCount = totalUsers // placeholder – we can refine later
        };

        return Ok(response);
    }
}

/*🔐 How Role Claims Work
During token generation (Ticket 5), we added each role as a claim: new Claim(ClaimTypes.Role, role).

The [Authorize(Roles = "Admin")] attribute translates to a policy that checks if the user has any claim of type ClaimTypes.Role with value "Admin".

If the user has multiple roles, they can access any endpoint that requires any of those roles (e.g., [Authorize(Roles = "Admin,Manager")] would allow both).

*/
