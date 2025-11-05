using System.ComponentModel.DataAnnotations;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;

namespace HUNting.API.Models.Map
{
    public class MapPath
    {
        [Key] public int PathId { get; set; }

        public int CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public int? SocietyId { get; set; }
        public Society? Society { get; set; }

        [MaxLength(200)] public string? Name { get; set; }

        [MaxLength(20)] public string VisibilityCode { get; set; } = "PUBLIC";
        public MapVisibility? Visibility { get; set; }

        [MaxLength(50)] public string? StrokeColor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MapPathPoint> Points { get; set; } = new List<MapPathPoint>();
    }
}

