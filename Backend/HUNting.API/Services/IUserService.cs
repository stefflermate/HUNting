using HUNting.API.Models.Users;

namespace HUNting.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(int id);
        Task<ApplicationUser> CreateAsync(ApplicationUser user, string password, IEnumerable<string>? roles = null);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(int id);
    }
}
