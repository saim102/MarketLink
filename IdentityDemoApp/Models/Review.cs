using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int CustomerProfileId { get; set; }

        public int ProductId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false;

        public CustomerProfile? CustomerProfile { get; set; }

        public Product? Product { get; set; }

        public ICollection<ReviewResponse> ReviewResponses { get; set; }
            = new List<ReviewResponse>();
    }
}