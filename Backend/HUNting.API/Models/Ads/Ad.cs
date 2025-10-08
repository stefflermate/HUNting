using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.Ads
{
    public class Ad
    {
        [Key] public int AdId { get; set; }

        [Required, MaxLength(250)] public string Title { get; set; } = "";
        [MaxLength(4000)] public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
        [MaxLength(200)] public string? LocationText { get; set; }

        [MaxLength(30)] public string StatusCode { get; set; } = "ACTIVE";
        public AdStatus? Status { get; set; }

        [MaxLength(256)] public string? ContactEmail { get; set; }
        [MaxLength(50)] public string? ContactPhone { get; set; }

        public int CategoryId { get; set; }
        public AdCategory? Category { get; set; }

        public int? OwnerUserId { get; set; }
        public User? OwnerUser { get; set; }

        public int? OwnerSocietyId { get; set; }
        public Society? OwnerSociety { get; set; }

        public ICollection<AdImage> Images { get; set; } = new List<AdImage>();
    }
}
