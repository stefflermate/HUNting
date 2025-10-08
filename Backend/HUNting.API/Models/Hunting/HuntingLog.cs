using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.Hunting
{
    public class HuntingLog
    {
        [Key] public int HuntingLogId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime OccurredAt { get; set; }
        [MaxLength(200)] public string? LocationText { get; set; }
        [MaxLength(120)] public string? AnimalType { get; set; }
        public bool ShotFired { get; set; }
        [MaxLength(2000)] public string? Notes { get; set; }
        [MaxLength(1000)] public string? ImageUrl { get; set; }
        public bool ShareWithSociety { get; set; }
    }
}
