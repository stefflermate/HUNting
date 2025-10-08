using HUNting.API.Data;
using HUNting.API.Models.Hunting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HuntingLogsController : ControllerBase
    {
        private readonly HuntingContext _ctx;
        public HuntingLogsController(HuntingContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HuntingLog>>> GetAll()
            => Ok(await _ctx.HuntingLogs.Include(h => h.User).ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<HuntingLog>> Get(int id)
            => (await _ctx.HuntingLogs.FindAsync(id)) is { } h ? Ok(h) : NotFound();

        [HttpPost]
        public async Task<ActionResult<HuntingLog>> Create(HuntingLog log)
        { _ctx.HuntingLogs.Add(log); await _ctx.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = log.HuntingLogId }, log); }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, HuntingLog log)
        { if (id != log.HuntingLogId) return BadRequest(); _ctx.HuntingLogs.Update(log); await _ctx.SaveChangesAsync(); return NoContent(); }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        { var h = await _ctx.HuntingLogs.FindAsync(id); if (h == null) return NotFound(); _ctx.HuntingLogs.Remove(h); await _ctx.SaveChangesAsync(); return NoContent(); }
    }
}
