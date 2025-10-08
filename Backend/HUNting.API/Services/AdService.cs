using HUNting.API.Models.Ads;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Services
{
    public class AdService : IAdService
    {
        private readonly IGenericRepository<Ad> _repo;

        public AdService(IGenericRepository<Ad> repo) => _repo = repo;

        public Task<IEnumerable<Ad>> GetAllAsync() =>
            _repo.GetAllAsync(a => a.Category!, a => a.Images, a => a.Status!);

        public Task<Ad?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task AddAsync(Ad ad)
        {
            await _repo.AddAsync(ad);
            await _repo.SaveAsync();
        }

        public async Task UpdateAsync(Ad ad)
        {
            _repo.Update(ad);
            await _repo.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var found = await _repo.GetByIdAsync(id);
            if (found != null)
            {
                _repo.Delete(found);
                await _repo.SaveAsync();
            }
        }
    }
}
