using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.Map
{
    public class MapIconType
    {
        [Key] public int IconTypeId { get; set; }
        [MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(50)] public string? DefaultColor { get; set; }
        [MaxLength(200)] public string? Glyph { get; set; }
    }
}

