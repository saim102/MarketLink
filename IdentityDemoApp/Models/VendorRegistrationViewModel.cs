using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class VendorRegistrationViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Stall / Business Name")]
        public string StallBusinessName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}