namespace SecureJwtApi.DTOs.Auth;

// Response model after successful login/registration
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}