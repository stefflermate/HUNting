using System;
using System.Linq;
using HUNting.API.Data;
using HUNting.API.Models.Hunting;
using HUNting.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HuntingLogsController : ApiControllerBase
    {
        private readonly HuntingContext _ctx;
        private readonly IWebHostEnvironment _env;

        public HuntingLogsController(HuntingContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<HuntingLog>>> GetAll()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            IQueryable<HuntingLog> query = _ctx.HuntingLogs
                .AsNoTracking()
                .Include(h => h.User)
                .OrderByDescending(h => h.OccurredAt)
                .ThenByDescending(h => h.CreatedAt);

            var scope = Request.Query["scope"].ToString();
            var canSeeAll = IsAdministrator && scope.Equals("all", StringComparison.OrdinalIgnoreCase);
            if (!canSeeAll)
            {
                query = query.Where(h => h.UserId == userId.Value);
            }

            var logs = await query.ToListAsync();
            return Ok(logs);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<HuntingLog>> Get(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var log = await _ctx.HuntingLogs
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.HuntingLogId == id);
            if (log == null) return NotFound();

            if (log.UserId != userId && !IsAdministrator)
            {
                return Forbid();
            }

            return Ok(log);
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(10_485_760)]
        public async Task<ActionResult<HuntingLog>> Create([FromForm] HuntingLogCreateRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (IsSocietyAccount())
            {
                return BadRequest("Tarsasagi fiok nem hozhat letre naplobejegyzest.");
            }

            var log = new HuntingLog
            {
                UserId = userId.Value,
                OccurredAt = request.OccurredAt,
                LocationText = request.LocationText,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AnimalType = request.AnimalType,
                ShotFired = request.ShotFired,
                Notes = request.Notes,
                ShareWithSociety = request.ShareWithSociety,
                CreatedAt = DateTime.UtcNow
            };

            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    log.ImageUrl = await UploadStorage.SaveImageAsync(_env, "logs", request.Image);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            _ctx.HuntingLogs.Add(log);
            await _ctx.SaveChangesAsync();

            await _ctx.Entry(log).Reference(h => h.User).LoadAsync();

            return CreatedAtAction(nameof(Get), new { id = log.HuntingLogId }, log);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, HuntingLog log)
        {
            if (id != log.HuntingLogId) return BadRequest();

            var existing = await _ctx.HuntingLogs.FirstOrDefaultAsync(h => h.HuntingLogId == id);
            if (existing == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = IsAdministrator;
            if (!isAdmin && existing.UserId != userId.Value)
            {
                return Forbid();
            }

            existing.OccurredAt = log.OccurredAt;
            existing.LocationText = log.LocationText;
            existing.Latitude = log.Latitude;
            existing.Longitude = log.Longitude;
            existing.AnimalType = log.AnimalType;
            existing.ShotFired = log.ShotFired;
            existing.Notes = log.Notes;
            existing.ImageUrl = log.ImageUrl;
            existing.ShareWithSociety = log.ShareWithSociety;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _ctx.HuntingLogs.FirstOrDefaultAsync(h => h.HuntingLogId == id);
            if (existing == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = IsAdministrator;
            if (!isAdmin && existing.UserId != userId.Value)
            {
                return Forbid();
            }

            UploadStorage.DeleteImage(_env, "logs", existing.ImageUrl);
            _ctx.HuntingLogs.Remove(existing);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        public class HuntingLogCreateRequest
        {
            public DateTime OccurredAt { get; set; }
            public string? LocationText { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string? AnimalType { get; set; }
            public bool ShotFired { get; set; }
            public string? Notes { get; set; }
            public bool ShareWithSociety { get; set; }
            public IFormFile? Image { get; set; }
        }
    }
}
