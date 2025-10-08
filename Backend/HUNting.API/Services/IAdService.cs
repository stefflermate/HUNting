using HUNting.API.Models.Ads;

namespace HUNting.API.Services
{
    public interface IAdService
    {
        Task<IEnumerable<Ad>> GetAllAsync();
        Task<Ad?> GetByIdAsync(int id);
        Task AddAsync(Ad ad);
        Task UpdateAsync(Ad ad);
        Task DeleteAsync(int id);
    }
}
