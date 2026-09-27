using System.ComponentModel.DataAnnotations;

namespace IdentityDemoApp.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        [StringLength(100)]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string GeneratedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? GeneratedByUser { get; set; }
    }
}