using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class FavouriteMarket
    {
        [Key]
        public int FavouriteMarketId { get; set; }

        public int CustomerProfileId { get; set; }

        public int MarketId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CustomerProfile? CustomerProfile { get; set; }

        public Market? Market { get; set; }
    }
}