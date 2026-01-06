namespace AuthMvcApp.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        int GetExpiryMinutes();
    }
}
