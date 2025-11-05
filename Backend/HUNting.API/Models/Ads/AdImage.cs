using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Ads
{
    public class AdImage
    {
        [Key, Column("ImageId")] public int ImageId { get; set; }
        [Required] public int AdId { get; set; }
        [Required, MaxLength(400)] public string ImageUrl { get; set; } = "";
        public int SortOrder { get; set; } = 1;

        [JsonIgnore] public Ad? Ad { get; set; }
    }
}
