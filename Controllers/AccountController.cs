using Microsoft.AspNetCore.Mvc;
using AuthMvcApp.Models;
using AuthMvcApp.Services;

namespace AuthMvcApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IJsonDataService _dataService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public AccountController(IJsonDataService dataService, IOtpService otpService, IEmailService emailService)
        {
            _dataService = dataService;
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (IsAuthenticated()) return RedirectToAction("Index", "Dashboard");
            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existingUser = _dataService.GetUserByEmail(model.Email);
            if (existingUser != null && existingUser.IsVerified)
            {
                ModelState.AddModelError("Email", "Email already registered. Please login.");
                return View(model);
            }

            var otp = _otpService.GenerateOtp();

            if (existingUser != null && !existingUser.IsVerified)
            {
                existingUser.Name = model.Name;
                existingUser.Password = model.Password;
                existingUser.Otp = otp;
                existingUser.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
                _dataService.UpdateUser(existingUser);
            }
            else
            {
                var user = new UserModel
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    Otp = otp,
                    OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes()),
                    IsVerified = false,
                    IsAddedUser = false
                };
                _dataService.SaveUser(user);
            }

            var sent = await _emailService.SendOtpEmailAsync(model.Email, otp);
            if (!sent)
            {
                ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                return View(model);
            }

            TempData["Email"] = model.Email;
            TempData["Success"] = $"OTP sent to {model.Email}. Check your inbox!";
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");
            
            TempData.Keep("Email");
            return View(new OtpVerificationModel { Email = email });
        }

        [HttpPost]
        public IActionResult VerifyOtp(OtpVerificationModel model)
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");

            model.Email = email;
            if (!ModelState.IsValid)
            {
                TempData.Keep("Email");
                return View(model);
            }

            var user = _dataService.GetUserByEmail(email);
            if (user == null)
            {
                TempData["Error"] = "User not found. Please register again.";
                return RedirectToAction("Register");
            }

            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.Now)
            {
                ModelState.AddModelError("Otp", "OTP expired. Please click Resend OTP.");
                TempData.Keep("Email");
                return View(model);
            }

            if (user.Otp != model.Otp)
            {
                ModelState.AddModelError("Otp", "Invalid OTP. Please try again.");
                TempData.Keep("Email");
                return View(model);
            }

            user.IsVerified = true;
            user.Otp = null;
            user.OtpExpiry = null;
            _dataService.UpdateUser(user);

            TempData["Success"] = "Email verified successfully! Please login.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp()
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");

            var user = _dataService.GetUserByEmail(email);
            if (user == null) return RedirectToAction("Register");

            var otp = _otpService.GenerateOtp();
            user.Otp = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
            _dataService.UpdateUser(user);

            await _emailService.SendOtpEmailAsync(email, otp);

            TempData["Email"] = email;
            TempData["Success"] = "New OTP sent!";
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated()) return RedirectToAction("Index", "Dashboard");
            return View(new LoginModel());
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _dataService.GetUserByEmail(model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError("", "Email not registered.");
                return View(model);
            }

            if (!user.IsVerified)
            {
                ModelState.AddModelError("", "Email not verified. Please verify first.");
                return View(model);
            }

            if (user.Password != model.Password)
            {
                ViewBag.ShowForgotPassword = true;
                ViewBag.UserEmail = model.Email;
                ModelState.AddModelError("", "Invalid password.");
                return View(model);
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult ForgotPassword(string? email)
        {
            return View(new ForgotPasswordModel { Email = email ?? "" });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _dataService.GetUserByEmail(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email not registered.");
                return View(model);
            }

            if (!user.IsVerified)
            {
                ModelState.AddModelError("Email", "Email not verified. Please register again.");
                return View(model);
            }

            var otp = _otpService.GenerateOtp();
            user.Otp = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
            _dataService.UpdateUser(user);

            var sent = await _emailService.SendResetCodeEmailAsync(model.Email, otp);
            if (!sent)
            {
                ModelState.AddModelError("", "Failed to send Reset Code. Please try again.");
                return View(model);
            }

            TempData["ResetEmail"] = model.Email;
            TempData["Success"] = $"Reset Code sent to {model.Email}. Check your inbox!";
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData["ResetEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            
            TempData.Keep("ResetEmail");
            return View(new ResetPasswordModel { Email = email });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            var email = TempData["ResetEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            model.Email = email;
            if (!ModelState.IsValid)
            {
                TempData.Keep("ResetEmail");
                return View(model);
            }

            var user = _dataService.GetUserByEmail(email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ForgotPassword");
            }

            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.Now)
            {
                ModelState.AddModelError("Otp", "OTP expired. Please request a new one.");
                TempData.Keep("ResetEmail");
                return View(model);
            }

            if (user.Otp != model.Otp)
            {
                ModelState.AddModelError("Otp", "Invalid OTP. Please try again.");
                TempData.Keep("ResetEmail");
                return View(model);
            }

            user.Password = model.NewPassword;
            user.Otp = null;
            user.OtpExpiry = null;
            _dataService.UpdateUser(user);

            TempData["Success"] = "Password reset successfully! Please login with your new password.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendResetOtp()
        {
            var email = TempData["ResetEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            var user = _dataService.GetUserByEmail(email);
            if (user == null) return RedirectToAction("ForgotPassword");

            var otp = _otpService.GenerateOtp();
            user.Otp = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
            _dataService.UpdateUser(user);

            await _emailService.SendResetCodeEmailAsync(email, otp);

            TempData["ResetEmail"] = email;
            TempData["Success"] = "New Reset Code sent!";
            return RedirectToAction("ResetPassword");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
    }
}
