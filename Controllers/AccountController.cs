/*
 * =====================================================
 * AccountController.cs - User Authentication Controller
 * =====================================================
 * 
 * Ye controller user authentication handle karta hai:
 * - Register (new user signup)
 * - Login (existing user signin)
 * - Logout
 * - OTP Verification
 * - Forgot Password
 * 
 * Author: Harsh Rathod
 * =====================================================
 */

using Microsoft.AspNetCore.Mvc;
using AuthMvcApp.Models;
using AuthMvcApp.Services;

namespace AuthMvcApp.Controllers
{
    public class AccountController : Controller
    {
        // ========== SERVICES (Dependency Injection) ==========
        // Ye services constructor mein automatically inject hoti hain
        private readonly IJsonDataService _dataService;      // Users data read/write
        private readonly IUserDataService _userDataService;  // Added users data
        private readonly IOtpService _otpService;            // OTP generate karna
        private readonly IEmailService _emailService;        // Email bhejne ke liye

        // Constructor - Jab controller banta hai tab ye services milti hain
        public AccountController(
            IJsonDataService dataService, 
            IUserDataService userDataService,
            IOtpService otpService, 
            IEmailService emailService)
        {
            _dataService = dataService;
            _userDataService = userDataService;
            _otpService = otpService;
            _emailService = emailService;
        }

        // ========================================
        // REGISTER - New User Signup
        // ========================================
        
        // GET: /Account/Register - Register page dikhana
        [HttpGet]
        public IActionResult Register()
        {
            // Agar user already logged in hai to dashboard bhejo
            if (IsAuthenticated()) 
                return RedirectToAction("Index", "Dashboard");
            
            // Empty form dikhao
            return View(new RegisterModel());
        }

        // POST: /Account/Register - Form submit hone par
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            // Step 1: Form validation check
            if (!ModelState.IsValid) 
                return View(model);

            // Step 2: Check karo email already registered hai ya nahi
            var existingUser = _dataService.GetUserByEmail(model.Email);
            if (existingUser != null && existingUser.IsVerified)
            {
                ModelState.AddModelError("Email", "Email already registered. Please login.");
                return View(model);
            }

            // Step 3: 6-digit OTP generate karo
            var otp = _otpService.GenerateOtp();

            // Step 4: User data save karo
            if (existingUser != null && !existingUser.IsVerified)
            {
                // Agar unverified user hai to update karo
                existingUser.Name = model.Name;
                existingUser.Password = model.Password;
                existingUser.Otp = otp;
                existingUser.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
                _dataService.UpdateUser(existingUser);
            }
            else
            {
                // Naya user create karo
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

            // Step 5: OTP email bhejo
            var sent = await _emailService.SendOtpEmailAsync(model.Email, otp);
            if (!sent)
            {
                ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                return View(model);
            }

            // Step 6: OTP verification page par bhejo
            TempData["Email"] = model.Email;
            TempData["Success"] = $"OTP sent to {model.Email}. Check your inbox!";
            return RedirectToAction("VerifyOtp");
        }

        // ========================================
        // OTP VERIFICATION
        // ========================================
        
        // GET: /Account/VerifyOtp - OTP enter karne ka page
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            // Email TempData se lo
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email)) 
                return RedirectToAction("Register");
            
            TempData.Keep("Email"); // Email ko next request tak rakhna
            return View(new OtpVerificationModel { Email = email });
        }

        // POST: /Account/VerifyOtp - OTP verify karna
        [HttpPost]
        public IActionResult VerifyOtp(OtpVerificationModel model)
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email)) 
                return RedirectToAction("Register");

            model.Email = email;
            
            // Form validation
            if (!ModelState.IsValid)
            {
                TempData.Keep("Email");
                return View(model);
            }

            // User dhundho
            var user = _dataService.GetUserByEmail(email);
            if (user == null)
            {
                TempData["Error"] = "User not found. Please register again.";
                return RedirectToAction("Register");
            }

            // OTP expiry check
            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.Now)
            {
                ModelState.AddModelError("Otp", "OTP expired. Please click Resend OTP.");
                TempData.Keep("Email");
                return View(model);
            }

            // OTP match check
            if (user.Otp != model.Otp)
            {
                ModelState.AddModelError("Otp", "Invalid OTP. Please try again.");
                TempData.Keep("Email");
                return View(model);
            }

            // OTP sahi hai - User verify karo
            user.IsVerified = true;
            user.Otp = null;        // OTP clear karo
            user.OtpExpiry = null;
            _dataService.UpdateUser(user);

            TempData["Success"] = "Email verified successfully! Please login.";
            return RedirectToAction("Login");
        }

        // POST: /Account/ResendOtp - Naya OTP bhejo
        [HttpPost]
        public async Task<IActionResult> ResendOtp()
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email)) 
                return RedirectToAction("Register");

            var user = _dataService.GetUserByEmail(email);
            if (user == null) 
                return RedirectToAction("Register");

            // Naya OTP generate aur save karo
            var otp = _otpService.GenerateOtp();
            user.Otp = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
            _dataService.UpdateUser(user);

            // Email bhejo
            await _emailService.SendOtpEmailAsync(email, otp);

            TempData["Email"] = email;
            TempData["Success"] = "New OTP sent!";
            return RedirectToAction("VerifyOtp");
        }

        // ========================================
        // LOGIN - User Signin
        // ========================================
        
        // GET: /Account/Login - Login page
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated()) 
                return RedirectToAction("Index", "Dashboard");
            
            return View(new LoginModel());
        }

        // POST: /Account/Login - Login form submit
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            // ===== STEP 1: Pehle registered users mein check karo (users.json) =====
            var user = _dataService.GetUserByEmail(model.Email);
            
            if (user != null)
            {
                // User mila - ab verify aur password check karo
                if (!user.IsVerified)
                {
                    ModelState.AddModelError("", "Email not verified. Please verify first.");
                    return View(model);
                }

                if (user.Password != model.Password)
                {
                    // Password galat - Forgot Password option dikhao
                    ViewBag.ShowForgotPassword = true;
                    ViewBag.UserEmail = model.Email;
                    ModelState.AddModelError("", "Invalid password.");
                    return View(model);
                }

                // ✅ Login successful - Session mein save karo
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                
                return RedirectToAction("Index", "Dashboard");
            }

            // ===== STEP 2: Added users mein check karo (userdata.json) =====
            var addedUser = _userDataService.GetAllUsers()
                .FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

            if (addedUser == null)
            {
                ModelState.AddModelError("", "Email not registered.");
                return View(model);
            }

            if (addedUser.Password != model.Password)
            {
                ViewBag.ShowForgotPassword = true;
                ViewBag.UserEmail = model.Email;
                ModelState.AddModelError("", "Invalid password.");
                return View(model);
            }

            // ✅ Login successful for added user
            HttpContext.Session.SetString("UserId", addedUser.Id.ToString());
            HttpContext.Session.SetString("UserName", addedUser.Name);
            HttpContext.Session.SetString("UserEmail", addedUser.Email);
            
            return RedirectToAction("Index", "Dashboard");
        }

        // ========================================
        // FORGOT PASSWORD
        // ========================================
        
        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword(string? email)
        {
            return View(new ForgotPasswordModel { Email = email ?? "" });
        }

        // POST: /Account/ForgotPassword - Reset code bhejo
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);

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

            // Reset code generate aur bhejo
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

        // GET: /Account/ResetPassword - New password enter karo
        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData["ResetEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) 
                return RedirectToAction("ForgotPassword");
            
            TempData.Keep("ResetEmail");
            return View(new ResetPasswordModel { Email = email });
        }

        // POST: /Account/ResetPassword - Password reset karo
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            var email = TempData["ResetEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) 
                return RedirectToAction("ForgotPassword");

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

            // OTP check
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

            // ✅ Password update karo
            user.Password = model.NewPassword;
            user.Otp = null;
            user.OtpExpiry = null;
            _dataService.UpdateUser(user);

            TempData["Success"] = "Password reset successfully! Please login with your new password.";
            return RedirectToAction("Login");
        }

        // POST: Resend Reset OTP
        [HttpPost]
        public async Task<IActionResult> ResendResetOtp()
        {
            var email = TempData["ResetEmail"]?.ToString();
            if (string.IsNullOrEmpty(email)) 
                return RedirectToAction("ForgotPassword");

            var user = _dataService.GetUserByEmail(email);
            if (user == null) 
                return RedirectToAction("ForgotPassword");

            var otp = _otpService.GenerateOtp();
            user.Otp = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
            _dataService.UpdateUser(user);

            await _emailService.SendResetCodeEmailAsync(email, otp);

            TempData["ResetEmail"] = email;
            TempData["Success"] = "New Reset Code sent!";
            return RedirectToAction("ResetPassword");
        }

        // ========================================
        // LOGOUT
        // ========================================
        public IActionResult Logout()
        {
            // Session clear karo - User logged out
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ========================================
        // HELPER METHOD
        // ========================================
        
        // Check karo user logged in hai ya nahi
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }
    }
}
