using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Ads
{
    public class AdImage
    {
        [Key, Column("ImageId")] public int ImageId { get; set; }   // ← PK név a DB-ben
        [Required] public int AdId { get; set; }
        [Required, MaxLength(1000)] public string ImageUrl { get; set; } = "";
        public int SortOrder { get; set; } = 1;

        [JsonIgnore] public Ad? Ad { get; set; }   // ← visszamutató kapcsolat ignorálása
    }
}
