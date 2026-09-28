using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class VendorProfile
    {
        [Key]
        public int VendorProfileId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FarmName { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [StringLength(100)]
        public string FarmDescription { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }

        public ICollection<VendorMarket> VendorMarkets { get; set; }
            = new List<VendorMarket>();

        public ICollection<Product> Products { get; set; }
            = new List<Product>();

        public ICollection<WeeklyStock> WeeklyStocks { get; set; }
            = new List<WeeklyStock>();

        public ICollection<PickupSlot> PickupSlots { get; set; }
            = new List<PickupSlot>();

        public ICollection<FavouriteVendor> FavouriteVendors { get; set; }
    = new List<FavouriteVendor>();
    }
}