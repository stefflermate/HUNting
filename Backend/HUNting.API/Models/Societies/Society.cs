using HUNting.API.Models.News;
using HUNting.API.Models.Users;
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
        public int? OwnerUserId { get; set; }
        [JsonIgnore] public ApplicationUser? Owner { get; set; }

        public ICollection<SocietyMember> Members { get; set; } = new List<SocietyMember>();
        public ICollection<SocietyMembershipRequest> MembershipRequests { get; set; } = new List<SocietyMembershipRequest>();
        public ICollection<SocietyPost> Posts { get; set; } = new List<SocietyPost>();

        [JsonIgnore] public ICollection<NewsFeed> News { get; set; } = new List<NewsFeed>();
    }
}
