using HUNting.API.Data;
using HUNting.API.Models.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly HuntingContext _ctx;
        public NewsController(HuntingContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsFeed>>> GetAll()
            => Ok(await _ctx.NewsFeed.Include(n => n.Visibility).ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NewsFeed>> Get(int id)
            => (await _ctx.NewsFeed.FindAsync(id)) is { } n ? Ok(n) : NotFound();

        [HttpPost]
        public async Task<ActionResult<NewsFeed>> Create(NewsFeed news)
        { _ctx.NewsFeed.Add(news); await _ctx.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = news.NewsFeedId }, news); }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, NewsFeed news)
        { if (id != news.NewsFeedId) return BadRequest(); _ctx.NewsFeed.Update(news); await _ctx.SaveChangesAsync(); return NoContent(); }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        { var n = await _ctx.NewsFeed.FindAsync(id); if (n == null) return NotFound(); _ctx.NewsFeed.Remove(n); await _ctx.SaveChangesAsync(); return NoContent(); }
    }
}
