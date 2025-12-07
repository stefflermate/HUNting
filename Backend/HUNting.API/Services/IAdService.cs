using HUNting.API.Models.Ads;

namespace HUNting.API.Services
{
    using HUNting.API.Models;

    public interface IAdService
    {
        Task<PagedResult<Ad>> GetPagedAsync(AdQueryOptions options);
        Task<Ad?> GetByIdAsync(int id);
        Task<IEnumerable<AdCategory>> GetCategoriesAsync();
        Task<IEnumerable<AdStatus>> GetStatusesAsync();
        Task AddAsync(Ad ad);
        Task UpdateAsync(Ad ad);
        Task DeleteAsync(int id);
        Task<AdImage> AddImageAsync(int adId, string imageUrl, int? sortOrder = null);
        Task<AdImage?> GetImageByIdAsync(int imageId);
        Task RemoveImageAsync(AdImage image);
    }
}
