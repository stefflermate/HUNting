using HUNting.API.Data;
using HUNting.API.Models.Societies;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Services
{
    public class SocietyService : ISocietyService
    {
        private readonly HuntingContext _ctx;

        public SocietyService(HuntingContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Society>> GetAllAsync() =>
            await _ctx.Societies
                .Include(s => s.Members)
                    .ThenInclude(m => m.User)
                .Include(s => s.Members)
                    .ThenInclude(m => m.Role)
                .Include(s => s.Owner)
                .ToListAsync();

        public async Task<IEnumerable<Society>> GetByUserAsync(int userId) =>
            await _ctx.Societies
                .Include(s => s.Members)
                .Where(s => s.Members.Any(m => m.UserId == userId))
                .AsNoTracking()
                .ToListAsync();

        public async Task<Society?> GetByIdAsync(int id) =>
            await _ctx.Societies
                .Include(s => s.Members)
                    .ThenInclude(m => m.User)
                .Include(s => s.Members)
                    .ThenInclude(m => m.Role)
                .Include(s => s.Owner)
                .FirstOrDefaultAsync(s => s.SocietyId == id);

        public async Task AddAsync(Society s)
        {
            _ctx.Societies.Add(s);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Society s)
        {
            _ctx.Societies.Update(s);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var s = await _ctx.Societies.FindAsync(id);
            if (s != null)
            {
                _ctx.Societies.Remove(s);
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SocietyMember>> GetMembersAsync(int societyId)
        {
            return await _ctx.SocietyMembers
                .Include(m => m.User)
                .Include(m => m.Role)
                .Where(m => m.SocietyId == societyId)
                .ToListAsync();
        }

        public async Task AddMemberAsync(SocietyMember member)
        {
            _ctx.SocietyMembers.Add(member);
            await _ctx.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int societyId, int userId)
        {
            var m = await _ctx.SocietyMembers.FindAsync(societyId, userId);
            if (m != null)
            {
                _ctx.SocietyMembers.Remove(m);
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<SocietyMembershipRequest> CreateMembershipRequestAsync(int societyId, int userId, string? message)
        {
            var existingMember = await _ctx.SocietyMembers.AnyAsync(m => m.SocietyId == societyId && m.UserId == userId);
            if (existingMember)
            {
                throw new InvalidOperationException("Mar tagja vagy a tarsasagnak.");
            }

            var request = await _ctx.SocietyMembershipRequests.FindAsync(societyId, userId);
            if (request != null)
            {
                if (request.Status == MembershipRequestStatus.Pending)
                {
                    throw new InvalidOperationException("Mar van fuggoben levo kerelmed.");
                }

                request.Status = MembershipRequestStatus.Pending;
                request.Message = message;
                request.CreatedAt = DateTime.UtcNow;
                request.ProcessedAt = null;
                request.ProcessedByUserId = null;
            }
            else
            {
                request = new SocietyMembershipRequest
                {
                    SocietyId = societyId,
                    UserId = userId,
                    Message = message,
                    Status = MembershipRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _ctx.SocietyMembershipRequests.Add(request);
            }

            await _ctx.SaveChangesAsync();
            return request;
        }

        public async Task<IEnumerable<SocietyMembershipRequest>> GetMembershipRequestsAsync(int societyId)
        {
            return await _ctx.SocietyMembershipRequests
                .Where(r => r.SocietyId == societyId && r.Status == MembershipRequestStatus.Pending)
                .Include(r => r.User)
                .Include(r => r.ProcessedBy)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<SocietyMembershipRequest?> GetMembershipRequestAsync(int societyId, int userId)
        {
            return await _ctx.SocietyMembershipRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.SocietyId == societyId && r.UserId == userId);
        }

        public async Task ApproveMembershipRequestAsync(int societyId, int userId, int approverUserId)
        {
            var request = await _ctx.SocietyMembershipRequests.FindAsync(societyId, userId);
            if (request == null)
            {
                throw new InvalidOperationException("A keres nem talalhato.");
            }

            if (request.Status == MembershipRequestStatus.Approved)
            {
                return;
            }

            request.Status = MembershipRequestStatus.Approved;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedByUserId = approverUserId;

            var isMember = await _ctx.SocietyMembers.AnyAsync(m => m.SocietyId == societyId && m.UserId == userId);
            if (!isMember)
            {
                var defaultRole = await _ctx.SocietyRoles.FirstOrDefaultAsync(r => r.Name == "Tag");
                var roleId = defaultRole?.RoleId ?? await _ctx.SocietyRoles.Select(r => r.RoleId).FirstAsync();
                _ctx.SocietyMembers.Add(new SocietyMember
                {
                    SocietyId = societyId,
                    UserId = userId,
                    RoleId = roleId,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task RejectMembershipRequestAsync(int societyId, int userId, int approverUserId)
        {
            var request = await _ctx.SocietyMembershipRequests.FindAsync(societyId, userId);
            if (request == null)
            {
                throw new InvalidOperationException("A keres nem talalhato.");
            }

            request.Status = MembershipRequestStatus.Rejected;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedByUserId = approverUserId;
            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<SocietyPost>> GetPostsAsync(int societyId)
        {
            return await _ctx.SocietyPosts
                .Where(p => p.SocietyId == societyId)
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<SocietyPost> AddPostAsync(SocietyPost post)
        {
            _ctx.SocietyPosts.Add(post);
            await _ctx.SaveChangesAsync();
            return post;
        }

        public async Task RemovePostAsync(int societyId, int postId)
        {
            var post = await _ctx.SocietyPosts.FirstOrDefaultAsync(p => p.SocietyPostId == postId && p.SocietyId == societyId);
            if (post == null) return;

            _ctx.SocietyPosts.Remove(post);
            await _ctx.SaveChangesAsync();
        }
    }
}
