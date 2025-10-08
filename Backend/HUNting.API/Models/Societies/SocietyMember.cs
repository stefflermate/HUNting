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

        [JsonIgnore] public Society? Society { get; set; }  // visszamutató
        public User? User { get; set; }                     // előre – ezt megjelenítjük
        public SocietyRole? Role { get; set; }              // előre – megjelenhet
    }
}
