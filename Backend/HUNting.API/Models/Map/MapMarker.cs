using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;

namespace HUNting.API.Models.Map
{
    public class MapMarker
    {
        [Key] public int MarkerId { get; set; }

        public int CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public int? SocietyId { get; set; }
        public Society? Society { get; set; }

        public int IconTypeId { get; set; }
        public MapIconType? IconType { get; set; }

        [Column(TypeName = "decimal(9,6)")] public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal Longitude { get; set; }

        [MaxLength(1000)] public string? Note { get; set; }

        [MaxLength(20)] public string VisibilityCode { get; set; } = "PUBLIC";
        public MapVisibility? Visibility { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

