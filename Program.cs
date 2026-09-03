using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureJwtApi.Data;
using SecureJwtApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.

// Register the DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity with our custom AppUser and IdentityRole
//Adds the core Identity services (UserManager, SignInManager, RoleManager, etc.). The generic parameters define our user type and role type.
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Configure Identity options (optional, but good to set sane defaults)
    //Enforces a strong password policy – users must have at least one digit.
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings (optional)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Ensures each user has a unique email address, which is also used as the username by default.
    options.User.RequireUniqueEmail = true;
})//Tells Identity to persist all user/role data using our EF Core DbContext.
.AddEntityFrameworkStores<ApplicationDbContext>()   // Use EF Core for storing Identity data
.AddDefaultTokenProviders();                         // Adds token providers for features like password reset and two‑factor authentication

// 3. *** NEW: Read JWT settings from configuration ***
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

// 4. *** NEW: Register Authentication with JWT Bearer ***
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>//Configures the JWT middleware with token validation parameters.
{
    //Uses the same secret key to validate the token's signature.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// Add Controllers (we are using Controller-based style)
builder.Services.AddControllers();

// Configure Swagger/OpenAPI (already present)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: Authentication & Authorization middleware will be added in Ticket 3.
// For now, we only have the services registered.
//Adds the middleware that reads the token from the Authorization header, validates it, and builds the User principal.
app.UseAuthentication();
//Enables role/policy-based authorization checks (used later for [Authorize(Roles = "Admin")]).
app.UseAuthorization();  

app.MapControllers();

app.Run();