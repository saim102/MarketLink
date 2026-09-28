using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string UserType { get; set; } = string.Empty;
   

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public VendorProfile? VendorProfile { get; set; }

        public CustomerProfile? CustomerProfile { get; set; }

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}