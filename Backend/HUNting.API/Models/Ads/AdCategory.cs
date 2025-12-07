using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Ads
{
    public class AdCategory
    {
        [Key] public int CategoryId { get; set; }
        [Required, MaxLength(60)] public string Name { get; set; } = "";

        [JsonIgnore] public ICollection<Ad> Ads { get; set; } = new List<Ad>();
    }
}
