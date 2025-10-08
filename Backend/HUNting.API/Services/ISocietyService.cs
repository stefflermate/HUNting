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
    }
}
