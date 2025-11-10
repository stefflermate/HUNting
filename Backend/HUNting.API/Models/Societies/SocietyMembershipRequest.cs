using HUNting.API.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace HUNting.API.Models.Societies
{
    public enum MembershipRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class SocietyMembershipRequest
    {
        [Required]
        public int SocietyId { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        public MembershipRequestStatus Status { get; set; } = MembershipRequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public int? ProcessedByUserId { get; set; }

        public Society? Society { get; set; }
        public ApplicationUser? User { get; set; }
        public ApplicationUser? ProcessedBy { get; set; }
    }
}
