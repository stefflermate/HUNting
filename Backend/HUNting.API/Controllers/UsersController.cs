using System.Linq;
using HUNting.API.Models.Users;
using HUNting.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserService users, UserManager<ApplicationUser> userManager)
        {
            _users = users;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
            var result = new List<UserResponse>(entities.Count);
            foreach (var entity in entities)
            {
                var roles = await _userManager.GetRolesAsync(entity);
                result.Add(UserResponse.From(entity, roles));
            }

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> Get(int id)
        {
            var entity = await _users.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(entity);
            return Ok(UserResponse.From(entity, roles));
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> Create(UserCreateRequest request)
        {
            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone
            };

            var created = await _users.CreateAsync(user, request.Password, request.Roles);
            var roles = await _userManager.GetRolesAsync(created);

            return CreatedAtAction(nameof(Get), new { id = created.Id }, UserResponse.From(created, roles));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UserUpdateRequest request)
        {
            if (id != request.Id) return BadRequest();

            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;

            await _users.UpdateAsync(user);

            if (request.Roles != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var toAdd = request.Roles.Except(currentRoles);
                var toRemove = currentRoles.Except(request.Roles);

                if (toRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, toRemove);
                }

                if (toAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, toAdd);
                }
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _users.DeleteAsync(id);
            return NoContent();
        }
    }

    public record UserCreateRequest(
        string FullName,
        string Email,
        string Password,
        string? Phone,
        IEnumerable<string>? Roles);

    public record UserUpdateRequest(
        int Id,
        string FullName,
        string Email,
        string? Phone,
        IEnumerable<string>? Roles);

    public record UserResponse(
        int Id,
        string FullName,
        string Email,
        string? Phone,
        IEnumerable<string> Roles,
        DateTime CreatedAt)
    {
        public static UserResponse From(ApplicationUser user, IEnumerable<string> roles) =>
            new(
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                user.Phone,
                roles,
                user.CreatedAt);
    }
}
