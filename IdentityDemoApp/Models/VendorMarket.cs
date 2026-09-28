using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class VendorMarket
    {
        [Key]
        public int VendorMarketId { get; set; }

        public int VendorProfileId { get; set; }

        public int MarketId { get; set; }

        public bool IsActive { get; set; } = true;

        public VendorProfile? VendorProfile { get; set; }

        public Market? Market { get; set; }
    }
}