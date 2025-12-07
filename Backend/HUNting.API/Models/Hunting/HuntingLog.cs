using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.Hunting
{
    public class HuntingLog
    {
        [Key, Column("LogId")] public int HuntingLogId { get; set; }
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime OccurredAt { get; set; }
        [MaxLength(200)] public string? LocationText { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal? Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal? Longitude { get; set; }
        [MaxLength(100)] public string? AnimalType { get; set; }
        public bool ShotFired { get; set; }
        public string? Notes { get; set; }
        [MaxLength(400)] public string? ImageUrl { get; set; }
        public bool ShareWithSociety { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
