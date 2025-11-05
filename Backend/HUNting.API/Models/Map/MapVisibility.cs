using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.Map
{
    public class MapVisibility
    {
        [Key, MaxLength(20)] public string VisibilityCode { get; set; } = "PUBLIC";
        [MaxLength(100)] public string? Description { get; set; }
    }
}

