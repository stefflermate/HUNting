using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.News
{
    public class NewsVisibility
    {
        [Key, Column("VisibilityCode"), MaxLength(20)]
        public string VisibilityCode { get; set; } = "MEMBERS";

        [MaxLength(200)]
        public string? Description { get; set; }

        // FONTOS: soha ne new ICollection<...>(), hanem new List<...>()
        [JsonIgnore]
        public ICollection<NewsFeed> News { get; set; } = new List<NewsFeed>();
    }
}
