using HUNting.API.Models.Societies;
using HUNting.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocietiesController : ControllerBase
    {
        private readonly ISocietyService _svc;
        public SocietiesController(ISocietyService svc) => _svc = svc;

        [HttpGet] public async Task<ActionResult<IEnumerable<Society>>> GetAll() => Ok(await _svc.GetAllAsync());
        [HttpGet("{id:int}")] public async Task<ActionResult<Society>> Get(int id)
            => (await _svc.GetByIdAsync(id)) is { } s ? Ok(s) : NotFound();

        [HttpPost] public async Task<ActionResult<Society>> Create(Society s)
        { await _svc.AddAsync(s); return CreatedAtAction(nameof(Get), new { id = s.SocietyId }, s); }

        [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, Society s)
        { if (id != s.SocietyId) return BadRequest(); await _svc.UpdateAsync(s); return NoContent(); }

        [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id)
        { await _svc.DeleteAsync(id); return NoContent(); }

        [HttpGet("{id:int}/members")]
        public async Task<ActionResult<IEnumerable<SocietyMember>>> Members(int id)
            => Ok(await _svc.GetMembersAsync(id));

        [HttpPost("{id:int}/members")]
        public async Task<IActionResult> AddMember(int id, SocietyMember m)
        { if (id != m.SocietyId) return BadRequest(); await _svc.AddMemberAsync(m); return NoContent(); }

        [HttpDelete("{id:int}/members/{userId:int}")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        { await _svc.RemoveMemberAsync(id, userId); return NoContent(); }
    }
}
