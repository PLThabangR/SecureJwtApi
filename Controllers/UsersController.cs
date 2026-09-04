using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureJwtApi.DTOs.User;
using SecureJwtApi.Models;

namespace SecureJwtApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] //  This makes ALL actions in this controller require authentication
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: api/users/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Get the user ID from the claims – the JWT middleware populates HttpContext.User.
        // We use the "sub" claim (or ClaimTypes.NameIdentifier) which we set during token generation.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            // This should not happen if the token is valid, but we handle it gracefully.
            return Unauthorized(new { Message = "User ID not found in token" });
        }

        // Fetch the user from the database using UserManager.
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            // The user may have been deleted after the token was issued.
            return Unauthorized(new { Message = "User not found" });
        }

        // Get the user's roles.
        var roles = await _userManager.GetRolesAsync(user);

        // Build the response DTO.
        var response = new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            Roles = roles
        };

        return Ok(response);
    }
}