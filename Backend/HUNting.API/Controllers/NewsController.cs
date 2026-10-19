using HUNting.API.Data;
using HUNting.API.Models.News;
using Microsoft.AspNetCore.Authorization;
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
            => Ok(await _ctx.NewsFeed
                .Include(n => n.Visibility)
                .Include(n => n.Society)
                .Include(n => n.Author)
                .ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NewsFeed>> Get(int id)
        {
            var news = await _ctx.NewsFeed
                .Include(n => n.Visibility)
                .Include(n => n.Society)
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.NewsFeedId == id);

            return news is null ? NotFound() : Ok(news);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<NewsFeed>> Create(NewsFeed news)
        {
            news.CreatedAt = DateTime.UtcNow;
            _ctx.NewsFeed.Add(news);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = news.NewsFeedId }, news);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, NewsFeed news)
        {
            if (id != news.NewsFeedId) return BadRequest();
            _ctx.NewsFeed.Update(news);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var n = await _ctx.NewsFeed.FindAsync(id);
            if (n == null) return NotFound();
            _ctx.NewsFeed.Remove(n);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
