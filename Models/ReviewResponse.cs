using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class ReviewResponse
    {
        [Key]
        public int ReviewResponseId { get; set; }

        public int ReviewId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Response { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Review? Review { get; set; }

        public ApplicationUser? User { get; set; }
    }
}