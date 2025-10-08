using HUNting.API.Models.Ads;
using HUNting.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly IAdService _svc;
        public AdsController(IAdService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ad>>> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ad>> Get(int id)
        {
            var ad = await _svc.GetByIdAsync(id);
            return ad == null ? NotFound() : Ok(ad);
        }

        [HttpPost]
        public async Task<ActionResult<Ad>> Create([FromBody] Ad ad)
        {
            await _svc.AddAsync(ad);
            return CreatedAtAction(nameof(Get), new { id = ad.AdId }, ad);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Ad ad)
        {
            if (id != ad.AdId) return BadRequest();
            await _svc.UpdateAsync(ad);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
    }
}
