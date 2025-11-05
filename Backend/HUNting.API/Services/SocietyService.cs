using HUNting.API.Data;
using HUNting.API.Models.Societies;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Services
{
    public class SocietyService : ISocietyService
    {
        private readonly HuntingContext _ctx;
        private readonly IGenericRepository<Society> _repo;

        public SocietyService(HuntingContext ctx, IGenericRepository<Society> repo)
        {
            _ctx = ctx;
            _repo = repo;
        }

        public async Task<IEnumerable<Society>> GetAllAsync() =>
            await _ctx.Societies
                .Include(s => s.Members)
                    .ThenInclude(m => m.User)
                .Include(s => s.Members)
                    .ThenInclude(m => m.Role)
                .ToListAsync();

        public async Task<Society?> GetByIdAsync(int id) =>
            await _ctx.Societies
                .Include(s => s.Members)
                    .ThenInclude(m => m.User)
                .Include(s => s.Members)
                    .ThenInclude(m => m.Role)
                .FirstOrDefaultAsync(s => s.SocietyId == id);

        public async Task AddAsync(Society s) { await _repo.AddAsync(s); await _repo.SaveAsync(); }
        public async Task UpdateAsync(Society s) { _repo.Update(s); await _repo.SaveAsync(); }
        public async Task DeleteAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id);
            if (s != null) { _repo.Delete(s); await _repo.SaveAsync(); }
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
    }
}
