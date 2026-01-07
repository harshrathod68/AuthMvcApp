using System.ComponentModel.DataAnnotations;

namespace AuthMvcApp.Models
{
    /// <summary>
    /// Model for user data stored in JSON file
    /// Used for CRUD operations in User Management
    /// </summary>
    public class UserDataModel
    {
        /// <summary>
        /// Unique identifier for the user (auto-incremented)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the user
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password for user login (stored in JSON, not shown in list)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// City of the user
        /// </summary>
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string? City { get; set; }

        /// <summary>
        /// Date when the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Date when the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Flag to indicate if user is verified (for login)
        /// </summary>
        public bool IsVerified { get; set; } = true;
    }

    /// <summary>
    /// Model for creating a new user
    /// </summary>
    public class CreateUserModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string? City { get; set; }
    }

    /// <summary>
    /// Model for editing an existing user
    /// </summary>
    public class EditUserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string? City { get; set; }
    }
}
