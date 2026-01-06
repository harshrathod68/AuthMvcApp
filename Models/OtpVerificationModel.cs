using System.ComponentModel.DataAnnotations;

namespace AuthMvcApp.Models
{
    public class OtpVerificationModel
    {
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be numeric")]
        public string Otp { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
