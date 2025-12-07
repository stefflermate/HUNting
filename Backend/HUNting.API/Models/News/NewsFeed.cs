using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.News
{
    public class NewsFeed
    {
        [Key, Column("PostId")] public int NewsFeedId { get; set; }

        public int SocietyId { get; set; }
        public Society? Society { get; set; }

        public int AuthorUserId { get; set; }
        public ApplicationUser? Author { get; set; }

        [MaxLength(200)] public string? Title { get; set; }
        public string? Content { get; set; }
        [MaxLength(400)] public string? ImageUrl { get; set; }

        [MaxLength(20)] public string VisibilityCode { get; set; } = "MEMBERS";
        public NewsVisibility? Visibility { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
