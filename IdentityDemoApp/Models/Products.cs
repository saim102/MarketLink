using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemoApp.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public int VendorProfileId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(20)]
        public string ModerationStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public VendorProfile? VendorProfile { get; set; }

        public Category? Category { get; set; }

        public ICollection<WeeklyStock> WeeklyStocks { get; set; }
            = new List<WeeklyStock>();

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();

        public ICollection<FavouriteProduct> FavouriteProducts { get; set; }
            = new List<FavouriteProduct>();

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();

        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}