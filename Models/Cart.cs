using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemoApp.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerProfileId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public CustomerProfile? CustomerProfile { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}