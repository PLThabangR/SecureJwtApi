using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SecureJwtApi.DTOs.Auth;
using SecureJwtApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace SecureJwtApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // UserManager and SignInManager, which are the official Identity abstractions.
    //1The primary service for user management – creates users, hashes passwords, finds users by email, adds/removes roles, etc. It encapsulates all Identity business logic.
    private readonly UserManager<AppUser> _userManager;
    //Used only for password verification (CheckPasswordSignInAsync). We do not use its cookie‑based sign‑in methods because we are a stateless API. This method validates the password hash without setting an authentication cookie.
    private readonly SignInManager<AppUser> _signInManager;
    //Reads the JwtSettings section from appsettings.json (SecretKey, Issuer, Audience, ExpiryMinutes). We inject it so the token generation is configurable without recompilation
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    //creating new identities in the system.
    //creating new identities in the system.
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        // Validate the model (automatic via [ApiController])

        // Create a new AppUser instance
        var user = new AppUser
        {
            UserName = request.Email,  // Identity uses UserName for login, we set it equal to Email
            Email = request.Email
        };

        // Attempt to create the user with the provided password
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // Return validation errors
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        // (Optional) Assign a default role like "User" – we'll skip for now (Ticket 8 handles roles)

        // Generate a JWT token for the newly registered user (so they can immediately use the API)
        var token = await GenerateJwtToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            Roles = await _userManager.GetRolesAsync(user)
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Find the user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Return 401 Unauthorized to avoid disclosing existence
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Verify the password using SignInManager
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {//means we are not incrementing failed attempt counters (which would trigger lockout)
          //  In a JWT API, we typically handle rate‑limiting at the middleware level instead of relying on Identity's lockout.
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Generate a JWT token
        var token = await GenerateJwtToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            Roles = await _userManager.GetRolesAsync(user)
        });
    }

    // Helper method to generate JWT token for a given user
    private async Task<string> GenerateJwtToken(AppUser user)
    {  // Retrieve JWT settings from appsettings.json
        var jwtSettings = _configuration.GetSection("JwtSettings");

        // SecretKey must be at least 32 characters (256 bits) for HMACSHA256.
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var issuer = jwtSettings["Issuer"]!;        // Who issued the token (our API)
        var audience = jwtSettings["Audience"]!;    // Who the token is intended for (our API)
        var expiryMinutes = Convert.ToDouble(jwtSettings["ExpiryMinutes"]); // Token lifetime

        // Get the user's roles (e.g., "Admin", "User") – these are stored in AspNetUserRoles table.
        var roles = await _userManager.GetRolesAsync(user);

        // Build the claims (payload) of the JWT.
        var claims = new List<Claim>
        {
            // "sub" (subject) – typically the user identifier.
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            
            // "email" – standard email claim.
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            
            // "jti" (JWT ID) – a unique identifier for this token to prevent replay attacks.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
            // "nameid" – another common claim for user ID (used by some authorization libraries).
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        // Add each role as a separate claim of type ClaimTypes.Role.
        // The [Authorize(Roles = "Admin")] attribute checks for this claim type.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create the symmetric security key and signing credentials.
        // HMACSHA256 is used – the same key is used to sign and validate the token.
        var key = new SymmetricSecurityKey(secretKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define the token properties (expiration, issuer, audience, claims).
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),   // All the claims we built
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes), // UTC expiry
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        // Use the JwtSecurityTokenHandler to create and write the token as a string.
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}