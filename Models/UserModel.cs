namespace AuthMvcApp.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Otp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsAddedUser { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
