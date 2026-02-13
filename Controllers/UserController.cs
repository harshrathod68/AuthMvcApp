using Microsoft.AspNetCore.Mvc;
using MyApps.Services;
using MyApps.Models;

namespace MyApps.Controllers
{
    /// <summary>
    /// Controller for user data management
    /// Handles CRUD operations for user data stored in JSON
    /// Includes OTP verification for adding new users
    /// </summary>
    public class UserController : Controller
    {
        private readonly IUserDataService _userDataService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IAccessLogService _accessLogService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public UserController(
            IUserDataService userDataService, 
            IOtpService otpService,
            IEmailService emailService,
            IAccessLogService accessLogService,
            ILogger<UserController> logger)
        {
            _userDataService = userDataService;
            _otpService = otpService;
            _emailService = emailService;
            _accessLogService = accessLogService;
            _logger = logger;
        }

        #region List Users

        /// <summary>
        /// Displays the list of all verified users (Admin only)
        /// </summary>
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied! Only Admin users can access Users page.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Admin: Show all verified users from userdata.json
            var users = _userDataService.GetAllUsers()
                .Where(u => u.IsVerified)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = userRole;
            ViewBag.IsAdmin = true;
            return View(users);
        }

        #endregion

        #region Create User with OTP

        /// <summary>
        /// Displays the create user form
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new CreateUserModel());
        }

        /// <summary>
        /// Handles the create user form submission - sends OTP
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for duplicate email in userdata.json
            if (_userDataService.EmailExists(model.Email))
            {
                ModelState.AddModelError("Email", "This email already exists");
                return View(model);
            }

            try
            {
                // Generate OTP
                var otp = _otpService.GenerateOtp();

                // Create user with unverified status
                var user = new UserDataModel
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    Role = model.Role, // Save selected role
                    City = model.City,
                    Otp = otp,
                    OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes()),
                    IsVerified = false
                };

                _userDataService.AddUser(user);

                // Send OTP email
                var sent = await _emailService.SendOtpEmailAsync(model.Email, otp);
                if (!sent)
                {
                    // Remove unverified user if email fails
                    var addedUser = _userDataService.GetAllUsers()
                        .FirstOrDefault(u => u.Email == model.Email && !u.IsVerified);
                    if (addedUser != null)
                    {
                        _userDataService.DeleteUser(addedUser.Id);
                    }
                    
                    ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                    return View(model);
                }

                // Store email in TempData for verification
                TempData["AddUserEmail"] = model.Email;
                TempData["Success"] = $"OTP sent to {model.Email}. Please verify!";
                
                _logger.LogInformation("OTP sent for new user: {Email}", model.Email);
                
                return RedirectToAction(nameof(VerifyOtp));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "An error occurred while creating the user");
                return View(model);
            }
        }

        /// <summary>
        /// Displays OTP verification page for new user
        /// </summary>
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var email = TempData["AddUserEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Create));
            }

            TempData.Keep("AddUserEmail");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            
            return View(new UserOtpVerificationModel { Email = email });
        }

        /// <summary>
        /// Handles OTP verification for new user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(UserOtpVerificationModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var email = TempData["AddUserEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Create));
            }

            model.Email = email;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid)
            {
                TempData.Keep("AddUserEmail");
                return View(model);
            }

            // Find unverified user
            var user = _userDataService.GetAllUsers()
                .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && !u.IsVerified);

            if (user == null)
            {
                TempData["Error"] = "User not found. Please try again.";
                return RedirectToAction(nameof(Create));
            }

            // Check OTP expiry
            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.Now)
            {
                ModelState.AddModelError("Otp", "OTP expired. Please click Resend OTP.");
                TempData.Keep("AddUserEmail");
                return View(model);
            }

            // Verify OTP
            if (user.Otp != model.Otp)
            {
                ModelState.AddModelError("Otp", "Invalid OTP. Please try again.");
                TempData.Keep("AddUserEmail");
                return View(model);
            }

            // Mark user as verified
            user.IsVerified = true;
            user.Otp = null;
            user.OtpExpiry = null;
            _userDataService.UpdateUser(user);

            TempData["Success"] = "User verified and added successfully!";
            _logger.LogInformation("User verified: {Email}", email);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Resends OTP for user verification
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var email = TempData["AddUserEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Create));
            }

            var user = _userDataService.GetAllUsers()
                .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && !u.IsVerified);

            if (user == null)
            {
                return RedirectToAction(nameof(Create));
            }

            // Generate new OTP
            var otp = _otpService.GenerateOtp();
            user.Otp = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(_otpService.GetExpiryMinutes());
            _userDataService.UpdateUser(user);

            // Send OTP
            await _emailService.SendOtpEmailAsync(email, otp);

            TempData["AddUserEmail"] = email;
            TempData["Success"] = "New OTP sent!";
            
            return RedirectToAction(nameof(VerifyOtp));
        }

        #endregion

        #region Edit User

        /// <summary>
        /// Displays the edit user form
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _userDataService.GetUserById(id);
            
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditUserModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                City = user.City
            };

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(model);
        }

        /// <summary>
        /// Handles the edit user form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_userDataService.EmailExists(model.Email, model.Id))
            {
                ModelState.AddModelError("Email", "This email already exists");
                return View(model);
            }

            try
            {
                var user = new UserDataModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password ?? string.Empty,
                    City = model.City
                };

                var result = _userDataService.UpdateUser(user);
                
                if (result)
                {
                    TempData["Success"] = "User updated successfully!";
                    _logger.LogInformation("User updated: {Id}", model.Id);
                }
                else
                {
                    TempData["Error"] = "Failed to update user";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Id}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating the user");
                return View(model);
            }
        }

        #endregion

        #region View User Details

        /// <summary>
        /// Displays user details with login history
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _userDataService.GetUserById(id);
            
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            // Get user's login history
            var allLogs = await _accessLogService.GetDailyAccessSummaryAsync();
            var userLogs = allLogs
                .Where(log => log.AccessLogs.Any(l => l.UserEmail == user.Email))
                .Select(log => new
                {
                    Date = log.Date,
                    Count = log.AccessLogs.Count(l => l.UserEmail == user.Email),
                    Logs = log.AccessLogs.Where(l => l.UserEmail == user.Email).ToList()
                })
                .ToList();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.LoginHistory = userLogs;
            ViewBag.TotalLogins = userLogs.Sum(l => l.Count);
            
            return View(user);
        }

        #endregion

        #region Delete User

        /// <summary>
        /// Deletes a user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = _userDataService.DeleteUser(id);
                
                if (result)
                {
                    TempData["Success"] = "User deleted successfully!";
                    _logger.LogInformation("User deleted: {Id}", id);
                }
                else
                {
                    TempData["Error"] = "Failed to delete user";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Id}", id);
                TempData["Error"] = "An error occurred while deleting the user";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Helper Methods

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        #endregion

        #region My Profile (For Normal Users)

        /// <summary>
        /// Shows current user's own profile
        /// </summary>
        [HttpGet]
        public IActionResult MyProfile()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole") ?? "User";

            ViewBag.UserEmail = userEmail;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;

            return View();
        }

        #endregion

        #region Access History

        /// <summary>
        /// Display user access history (Admin only)
        /// Shows daily access logs with count
        /// </summary>
        public async Task<IActionResult> History()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied! Only Admin users can view access history.";
                return RedirectToAction("Index", "Dashboard");
            }

            var dailySummary = await _accessLogService.GetDailyAccessSummaryAsync();
            return View(dailySummary);
        }

        #endregion
    }
}
