namespace AuthMvcApp.Services
{
    public class OtpService : IOtpService
    {
        private readonly IConfiguration _configuration;
        private readonly Random _random = new();

        public OtpService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateOtp()
        {
            var length = int.Parse(_configuration["OtpSettings:Length"] ?? "6");
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;
            return _random.Next(min, max + 1).ToString();
        }

        public int GetExpiryMinutes()
        {
            return int.Parse(_configuration["OtpSettings:ExpiryMinutes"] ?? "5");
        }
    }
}
