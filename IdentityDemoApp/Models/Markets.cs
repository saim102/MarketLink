using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemoApp.Models
{
    public class Market
    {
        [Key]
        public int MarketId { get; set; }

        [Required]
        [StringLength(150)]
        public string MarketName { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Operating Days")]
        public string OperatingDays { get; set; } = string.Empty;

        [Display(Name = "Opening Time")]
        public TimeSpan? OpeningTime { get; set; }

        [Display(Name = "Closing Time")]
        public TimeSpan? ClosingTime { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [StringLength(500)]
        [Display(Name = "Map Link")]
        public string? MapLink { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<VendorMarket> VendorMarkets { get; set; }
            = new List<VendorMarket>();
    }
}