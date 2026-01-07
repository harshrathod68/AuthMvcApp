using Microsoft.AspNetCore.Mvc;
using AuthMvcApp.Services;
using AuthMvcApp.Models;

namespace AuthMvcApp.Controllers
{
    /// <summary>
    /// Controller for user data management
    /// Handles CRUD operations for user data stored in JSON
    /// </summary>
    public class UserController : Controller
    {
        private readonly IUserDataService _userDataService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userDataService">Service for user data operations</param>
        /// <param name="logger">Logger for logging operations</param>
        public UserController(IUserDataService userDataService, ILogger<UserController> logger)
        {
            _userDataService = userDataService;
            _logger = logger;
        }

        #region List Users

        /// <summary>
        /// Displays the list of all users
        /// </summary>
        /// <returns>User list view</returns>
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _userDataService.GetAllUsers()
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(users);
        }

        #endregion

        #region Create User

        /// <summary>
        /// Displays the create user form
        /// </summary>
        /// <returns>Create user view</returns>
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
        /// Handles the create user form submission
        /// </summary>
        /// <param name="model">User data from form</param>
        /// <returns>Redirect to list on success, form on error</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Validate model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for duplicate email
            if (_userDataService.EmailExists(model.Email))
            {
                ModelState.AddModelError("Email", "This email already exists");
                return View(model);
            }

            try
            {
                // Create new user
                var user = new UserDataModel
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    City = model.City,
                    IsVerified = true
                };

                _userDataService.AddUser(user);
                
                TempData["Success"] = "User created successfully!";
                _logger.LogInformation("User created: {Email}", model.Email);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "An error occurred while creating the user");
                return View(model);
            }
        }

        #endregion

        #region Edit User

        /// <summary>
        /// Displays the edit user form
        /// </summary>
        /// <param name="id">User ID to edit</param>
        /// <returns>Edit user view</returns>
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
        /// <param name="model">Updated user data</param>
        /// <returns>Redirect to list on success, form on error</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserModel model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Validate model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for duplicate email (excluding current user)
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
        /// Displays user details
        /// </summary>
        /// <param name="id">User ID to view</param>
        /// <returns>User details view</returns>
        public IActionResult Details(int id)
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

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(user);
        }

        #endregion

        #region Delete User

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>Redirect to list</returns>
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

        /// <summary>
        /// Checks if user is authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        #endregion
    }
}
