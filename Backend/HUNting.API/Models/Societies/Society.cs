using HUNting.API.Models.News;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Societies
{
    public class Society
    {
        [Key] public int SocietyId { get; set; }
        [Required, MaxLength(150)] public string Name { get; set; } = "";
        [Required, MaxLength(150)] public string Email { get; set; } = "";
        [Required, MaxLength(255)] public string PasswordHash { get; set; } = "";
        [MaxLength(20)] public string? Phone { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SocietyMember> Members { get; set; } = new List<SocietyMember>();

        [JsonIgnore] public ICollection<NewsFeed> News { get; set; } = new List<NewsFeed>();
    }
}
