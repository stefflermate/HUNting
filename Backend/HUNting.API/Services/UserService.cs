using HUNting.API.Models.Users;

namespace HUNting.API.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _repo;
        public UserService(IGenericRepository<User> repo) => _repo = repo;

        public Task<IEnumerable<User>> GetAllAsync() => _repo.GetAllAsync();
        public Task<User?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task AddAsync(User user) { await _repo.AddAsync(user); await _repo.SaveAsync(); }
        public async Task UpdateAsync(User user) { _repo.Update(user); await _repo.SaveAsync(); }
        public async Task DeleteAsync(int id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u != null) { _repo.Delete(u); await _repo.SaveAsync(); }
        }
    }
}
