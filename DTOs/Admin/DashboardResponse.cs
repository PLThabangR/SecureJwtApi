namespace SecureJwtApi.DTOs.Admin
{
    public class DashboardResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public int ActiveUsersCount { get; set; } // placeholder – we could count users later
    }
}
