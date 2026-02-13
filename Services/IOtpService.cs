namespace MyApps.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        int GetExpiryMinutes();
    }
}
