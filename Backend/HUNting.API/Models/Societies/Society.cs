using HUNting.API.Models.News;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Societies
{
    public class Society
    {
        [Key] public int SocietyId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = "";
        [MaxLength(256)] public string? Email { get; set; }
        [MaxLength(512)] public string? PasswordHash { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }

        // Ha a társaság listáknál NEM akarod visszaadni a taglistát, ezt is lehet ignorálni:
        // [JsonIgnore]
        public ICollection<SocietyMember> Members { get; set; } = new List<SocietyMember>();

        [JsonIgnore] public ICollection<NewsFeed> News { get; set; } = new List<NewsFeed>();
    }
}
