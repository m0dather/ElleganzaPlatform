using System.ComponentModel.DataAnnotations;

namespace ElleganzaPlatform.Areas.Identity.Models;

public class VendorRegisterViewModel
{
    // User Information
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Vendor Information
    [Required(ErrorMessage = "Vendor name is required")]
    [Display(Name = "Vendor/Store Name")]
    [StringLength(200)]
    public string VendorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vendor name (Arabic) is required")]
    [Display(Name = "Vendor/Store Name (Arabic)")]
    [StringLength(200)]
    public string VendorNameAr { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Description (Arabic)")]
    [StringLength(2000)]
    public string? DescriptionAr { get; set; }

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Contact Phone")]
    public string ContactPhone { get; set; } = string.Empty;
}
