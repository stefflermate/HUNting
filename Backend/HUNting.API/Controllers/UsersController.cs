using HUNting.API.Models.Users;
using HUNting.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _svc;
        public UsersController(IUserService svc) => _svc = svc;

        [HttpGet] public async Task<ActionResult<IEnumerable<User>>> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")] public async Task<ActionResult<User>> Get(int id)
            => (await _svc.GetByIdAsync(id)) is { } u ? Ok(u) : NotFound();

        [HttpPost] public async Task<ActionResult<User>> Create(User user)
        { await _svc.AddAsync(user); return CreatedAtAction(nameof(Get), new { id = user.UserId }, user); }

        [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, User user)
        { if (id != user.UserId) return BadRequest(); await _svc.UpdateAsync(user); return NoContent(); }

        [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id)
        { await _svc.DeleteAsync(id); return NoContent(); }
    }
}
