using HUNting.API.Models.Hunting;
using HUNting.API.Models.Societies;
using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.Users
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = "";

        [MaxLength(20)]
        public string? Phone { get; set; }

        // kollekciók helyes inicializálása
        public ICollection<SocietyMember> Societies { get; set; } = new List<SocietyMember>();
        public ICollection<HuntingLog> HuntingLogs { get; set; } = new List<HuntingLog>();
    }
}
