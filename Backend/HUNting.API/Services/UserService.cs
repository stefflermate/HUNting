using HUNting.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _userManager.Users.AsNoTracking().ToListAsync();
        }

        public Task<ApplicationUser?> GetByIdAsync(int id)
        {
            return _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser user, string password, IEnumerable<string>? roles = null)
        {
            user.UserName = user.Email;
            if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                user.PhoneNumber = user.Phone;
            }

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var message = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(message);
            }

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>(role));
                    }
                }

                var roleResult = await _userManager.AddToRolesAsync(user, roles);
                if (!roleResult.Succeeded)
                {
                    var message = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(message);
                }
            }

            return user;
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            user.UserName = user.Email;
            if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                user.PhoneNumber = user.Phone;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var message = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(message);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _userManager.FindByIdAsync(id.ToString());
            if (existing == null)
            {
                return;
            }

            var result = await _userManager.DeleteAsync(existing);
            if (!result.Succeeded)
            {
                var message = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(message);
            }
        }
    }
}
