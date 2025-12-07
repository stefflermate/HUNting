using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.Societies
{
    public class SocietyPost
    {
        [Key, Column("PostId")]
        public int SocietyPostId { get; set; }

        [Required]
        public int SocietyId { get; set; }

        [Required]
        public int AuthorUserId { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [MaxLength(400)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Society? Society { get; set; }
        public ApplicationUser? Author { get; set; }
    }
}
