using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemoApp.Models
{
    public class Vendor
    {
        [Key]
        public int VendorId { get; set; }

        // Identity User ID
        [Required]
        public string UserId { get; set; } = string.Empty;

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
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }
    }
}