namespace AuthMvcApp.Services
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otp);
        Task<bool> SendResetCodeEmailAsync(string toEmail, string resetCode);
    }
}
