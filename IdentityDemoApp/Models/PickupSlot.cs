using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class PickupSlot
    {
        [Key]
        public int PickupSlotId { get; set; }

        public int VendorProfileId { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public int MaxOrders { get; set; }

        public bool IsAvailable { get; set; } = true;

        public VendorProfile? VendorProfile { get; set; }

        public ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
}