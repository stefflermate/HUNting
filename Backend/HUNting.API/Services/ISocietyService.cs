using HUNting.API.Models.Societies;

namespace HUNting.API.Services
{
    public interface ISocietyService
    {
        Task<IEnumerable<Society>> GetAllAsync();
        Task<Society?> GetByIdAsync(int id);
        Task AddAsync(Society society);
        Task UpdateAsync(Society society);
        Task DeleteAsync(int id);

        Task<IEnumerable<SocietyMember>> GetMembersAsync(int societyId);
        Task AddMemberAsync(SocietyMember member);
        Task RemoveMemberAsync(int societyId, int userId);

        Task<IEnumerable<Society>> GetByUserAsync(int userId);
        Task<SocietyMembershipRequest> CreateMembershipRequestAsync(int societyId, int userId, string? message);
        Task<IEnumerable<SocietyMembershipRequest>> GetMembershipRequestsAsync(int societyId);
        Task<SocietyMembershipRequest?> GetMembershipRequestAsync(int societyId, int userId);
        Task ApproveMembershipRequestAsync(int societyId, int userId, int approverUserId);
        Task RejectMembershipRequestAsync(int societyId, int userId, int approverUserId);

        Task<IEnumerable<SocietyPost>> GetPostsAsync(int societyId);
        Task<SocietyPost> AddPostAsync(SocietyPost post);
        Task RemovePostAsync(int societyId, int postId);
    }
}
