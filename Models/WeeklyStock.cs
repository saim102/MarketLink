using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemoApp.Models
{
    public class WeeklyStock
    {
        [Key]
        public int WeeklyStockId { get; set; }

        public int ProductId { get; set; }

        public int VendorProfileId { get; set; }

        [Required]
        public DateTime WeekStartDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AvailableQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReservedQuantity { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }

        public VendorProfile? VendorProfile { get; set; }
    }
}