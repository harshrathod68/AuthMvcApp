using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MyApps.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "MyApps";

                // Validate configuration
                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogError("SMTP credentials not configured properly");
                    return false;
                }

                _logger.LogInformation($"Attempting to send OTP to: {toEmail}");
                _logger.LogInformation($"Using SMTP: {smtpHost}:{smtpPort}, User: {smtpUser}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = "Email Verification OTP - MyApps";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
    <div style='max-width: 500px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        <h2 style='color: #333; text-align: center;'>🔐 Email Verification OTP</h2>
        <p>Hello,</p>
        <p>Your One-Time Password (OTP) for email verification is:</p>
        <div style='background: #28a745; color: white; font-size: 36px; letter-spacing: 12px; text-align: center; padding: 25px; border-radius: 8px; margin: 25px 0;'>
            <strong>{otp}</strong>
        </div>
        <p>This OTP is valid for <strong>5 minutes</strong>.</p>
        <p style='color: #dc3545; font-size: 13px;'>⚠️ Do not share this OTP with anyone.</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
        <p style='color: #666; font-size: 12px; text-align: center;'>
            If you didn't request this OTP, please ignore this email.<br>
            © 2026 MyApps
        </p>
    </div>
</body>
</html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Set timeout
                client.Timeout = 30000; // 30 seconds
                
                // Connect to Gmail SMTP
                _logger.LogInformation("Connecting to SMTP server...");
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                _logger.LogInformation("Connected successfully");
                
                // Authenticate
                _logger.LogInformation("Authenticating...");
                await client.AuthenticateAsync(smtpUser, smtpPass);
                _logger.LogInformation("Authenticated successfully");
                
                // Send email
                _logger.LogInformation("Sending email...");
                await client.SendAsync(message);
                _logger.LogInformation("Email sent successfully");
                
                // Disconnect
                await client.DisconnectAsync(true);

                _logger.LogInformation($"OTP sent successfully to {toEmail}");
                return true;
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                _logger.LogError($"Authentication failed: {authEx.Message}");
                _logger.LogError("Please check your Gmail App Password. Generate a new one if needed.");
                return false;
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                _logger.LogError($"Network error: {socketEx.Message}");
                _logger.LogError("Please check your internet connection.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> SendResetCodeEmailAsync(string toEmail, string resetCode)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "MyApps";

                _logger.LogInformation($"Attempting to send Reset Code to: {toEmail}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = "Password Reset Code - MyApps";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
    <div style='max-width: 500px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        <h2 style='color: #333; text-align: center;'>🔒 Password Reset Code</h2>
        <p>Hello,</p>
        <p>You have requested to reset your password. Use the code below to reset your password:</p>
        <div style='background: linear-gradient(135deg, #f5af19 0%, #f12711 100%); color: white; font-size: 36px; letter-spacing: 12px; text-align: center; padding: 25px; border-radius: 8px; margin: 25px 0;'>
            <strong>{resetCode}</strong>
        </div>
        <p>This Reset Code is valid for <strong>5 minutes</strong>.</p>
        <p style='color: #dc3545; font-size: 13px;'>⚠️ If you didn't request this, please ignore this email. Your password will remain unchanged.</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
        <p style='color: #666; font-size: 12px; text-align: center;'>
            © 2026 MyApps - Password Reset Request
        </p>
    </div>
</body>
</html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Reset Code sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send reset code email: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}
