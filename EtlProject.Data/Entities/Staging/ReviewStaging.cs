using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Staging
{
    [Table("ReviewStaging", Schema = "staging")]
    public class ReviewStaging
    {
        [Key]
        public int Id { get; set; }
        public string? SourceType { get; set; } // CSV, DB, API
        public string? ReviewId { get; set; }
        public string? ClientId { get; set; }
        public string? ProductId { get; set; }
        public string? Comment { get; set; }
        public decimal? Rating { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime ExtractionDate { get; set; } = DateTime.UtcNow;
    }
}
