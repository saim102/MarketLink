using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class FavouriteProduct
    {
        [Key]
        public int FavouriteProductId { get; set; }

        public int CustomerProfileId { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CustomerProfile? CustomerProfile { get; set; }

        public Product? Product { get; set; }
    }
}