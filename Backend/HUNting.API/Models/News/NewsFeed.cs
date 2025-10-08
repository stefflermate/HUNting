using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.News
{
    public class NewsFeed
    {
        [Key] public int NewsFeedId { get; set; }

        public int SocietyId { get; set; }
        public Society? Society { get; set; }

        public int AuthorUserId { get; set; }
        public User? Author { get; set; }

        [Required, MaxLength(300)] public string Title { get; set; } = "";
        [MaxLength(4000)] public string? Content { get; set; }
        [MaxLength(1000)] public string? ImageUrl { get; set; }

        [MaxLength(30)] public string VisibilityCode { get; set; } = "PUBLIC";
        public NewsVisibility? Visibility { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
