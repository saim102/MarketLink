using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class CustomerProfile
    {
        [Key]
        public int CustomerProfileId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(30)]
        public string ContactNumber { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        // Customer approval
        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }

        public ICollection<Order> Orders { get; set; }
            = new List<Order>();

        public Cart? Cart { get; set; }

        public ICollection<FavouriteVendor> FavouriteVendors { get; set; }
            = new List<FavouriteVendor>();

        public ICollection<FavouriteProduct> FavouriteProducts { get; set; }
            = new List<FavouriteProduct>();

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();
    }
}