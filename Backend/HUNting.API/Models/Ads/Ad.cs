using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.Ads
{
    public class Ad
    {
        [Key] public int AdId { get; set; }

        [Required, MaxLength(200)] public string Title { get; set; } = "";
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public AdCategory? Category { get; set; }

        [Column(TypeName = "decimal(12,2)")] public decimal? Price { get; set; }
        [MaxLength(200)] public string? LocationText { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal? Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal? Longitude { get; set; }

        [MaxLength(20)] public string StatusCode { get; set; } = "ACTIVE";
        public AdStatus? Status { get; set; }

        [MaxLength(150)] public string? ContactEmail { get; set; }
        [MaxLength(30)] public string? ContactPhone { get; set; }

        public int? OwnerUserId { get; set; }
        public ApplicationUser? OwnerUser { get; set; }

        public int? OwnerSocietyId { get; set; }
        public Society? OwnerSociety { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AdImage> Images { get; set; } = new List<AdImage>();
    }
}
