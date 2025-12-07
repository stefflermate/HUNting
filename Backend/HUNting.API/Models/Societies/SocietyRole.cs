using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HUNting.API.Models.Societies
{
    public class SocietyRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [JsonIgnore]
        public ICollection<SocietyMember> Members { get; set; } = new List<SocietyMember>();
    }
}
