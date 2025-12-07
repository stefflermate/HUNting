using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Societies
{
    public class SocietyMember
    {
        [Required] public int SocietyId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public int RoleId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore] public Society? Society { get; set; }
        public ApplicationUser? User { get; set; }
        public SocietyRole? Role { get; set; }
    }
}
