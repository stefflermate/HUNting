using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Ads
{
    public class AdStatus
    {
        [Key, MaxLength(20)] public string StatusCode { get; set; } = "ACTIVE";
        [MaxLength(200)] public string? Description { get; set; }

        [JsonIgnore] public ICollection<Ad> Ads { get; set; } = new List<Ad>();
    }
}
