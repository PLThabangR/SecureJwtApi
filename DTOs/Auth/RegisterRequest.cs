namespace SecureJwtApi.DTOs.Auth;

// Request model for user registration
public class RegisterUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Optionally, we can add FirstName, LastName later – but we keep it minimal for now.
}