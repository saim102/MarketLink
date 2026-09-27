using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class FavouriteVendor
    {
        [Key]
        public int FavouriteVendorId { get; set; }

        public int CustomerProfileId { get; set; }

        public int VendorProfileId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CustomerProfile? CustomerProfile { get; set; }

        public VendorProfile? VendorProfile { get; set; }
    }
}