using HUNting.API.Models.Hunting;
using HUNting.API.Models.Societies;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.Users
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SocietyMember> Societies { get; set; } = new List<SocietyMember>();
        public ICollection<HuntingLog> HuntingLogs { get; set; } = new List<HuntingLog>();
    }
}
