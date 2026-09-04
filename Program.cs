using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SecureJwtApi.Data;
using SecureJwtApi.Models;

// ----- 1. BUILD THE APPLICATION HOST -----
var builder = WebApplication.CreateBuilder(args);

// ----- 2. CONFIGURE SERVICES (Dependency Injection) -----

// 2a. Register Entity Framework DbContext with SQL Server.
// The connection string is read from appsettings.json ("DefaultConnection").
// AddDbContext registers the DbContext as a scoped service.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2b. Register ASP.NET Core Identity services.
// We use AppUser as our custom user class and IdentityRole as the role class.
// Options: enforce strong password rules, unique emails, lockout settings.
// AddEntityFrameworkStores<ApplicationDbContext>() tells Identity to persist data using our EF Core context.
// AddDefaultTokenProviders() adds providers for password reset, two-factor authentication, etc.
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password strength requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false; // Allows simple passwords like "Test123!"
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings (optional, but useful for additional security)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Ensure each email is unique (used as the login identifier)
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 2c. Configure JWT Authentication.
// Read the JWT settings from appsettings.json (SecretKey, Issuer, Audience, ExpiryMinutes).
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

// AddAuthentication sets the default authentication and challenge schemes to JWT Bearer.
// AddJwtBearer configures the middleware to validate incoming tokens.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // TokenValidationParameters defines what to validate in the token.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                // Ensure the token was issued by our trusted issuer
        ValidateAudience = true,              // Ensure the token is intended for our API
        ValidateLifetime = true,              // Ensure the token hasn't expired
        ValidateIssuerSigningKey = true,      // Ensure the signature is valid (using the secret key)
        ValidIssuer = jwtSettings["Issuer"],  // The issuer we expect
        ValidAudience = jwtSettings["Audience"], // The audience we expect
        IssuerSigningKey = new SymmetricSecurityKey(secretKey) // The symmetric key used for signing
    };
});

// 2d. Add Controllers (we are using Controller-based API style).
builder.Services.AddControllers();

// 2e. Add Swagger / OpenAPI with JWT support.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Define the security scheme for Swagger UI: Bearer token.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",                      // The header name
        Type = SecuritySchemeType.ApiKey,            // API key style
        Scheme = "Bearer",                           // The scheme name
        BearerFormat = "JWT",                        // Format of the token
        In = ParameterLocation.Header,               // Where to send the token (in the header)
        Description = "Enter 'Bearer' followed by a space and your JWT token.\nExample: \"Bearer eyJhbGci...\""
    });

    // Require the Bearer scheme globally for all endpoints.
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()   // No scopes required for our simple scenario
        }
    });
});

// ----- 3. BUILD THE APPLICATION PIPELINE -----
var app = builder.Build();

// 3a. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI only in development for security.
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3b. Redirect HTTP to HTTPS (enforced in production).
app.UseHttpsRedirection();

// 3c. Add Authentication and Authorization middleware.
// UseAuthentication must come before UseAuthorization.
app.UseAuthentication(); // Reads the JWT from the Authorization header and populates HttpContext.User
app.UseAuthorization();  // Enforces [Authorize] attributes on controllers/actions

// 3d. Map controller routes.
app.MapControllers();

// ----- 4. SEED THE DATABASE WITH ROLES AND A DEFAULT ADMIN USER -----
// This is executed once at startup. It ensures the Admin role exists and creates
// an admin user (admin@example.com / Admin123!) if it doesn't already exist.
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// ----- 5. RUN THE APPLICATION -----
app.Run();