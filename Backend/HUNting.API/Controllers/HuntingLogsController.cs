using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using HUNting.API.Data;
using HUNting.API.Models.Hunting;
using HUNting.API.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HuntingLogsController : ControllerBase
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
            var canSeeAll = User.IsInRole("Administrator") && scope.Equals("all", StringComparison.OrdinalIgnoreCase);
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

            if (log.UserId != userId && !User.IsInRole("Administrator"))
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
                    log.ImageUrl = await SaveImageAsync(request.Image);
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

            var isAdmin = User.IsInRole("Administrator");
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

            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && existing.UserId != userId.Value)
            {
                return Forbid();
            }

            DeleteImageFile(existing.ImageUrl);
            _ctx.HuntingLogs.Remove(existing);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        private int? GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out var id) ? id : null;
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(extension))
            {
                throw new ArgumentException("Csak JPG, JPEG, PNG vagy WEBP fajl tamogatott.");
            }

            var root = EnsureUploadsDirectory();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var absolutePath = Path.Combine(root, fileName);

            await using (var stream = System.IO.File.Create(absolutePath))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/logs/" + fileName;
        }

        private string EnsureUploadsDirectory()
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                Directory.CreateDirectory(webRoot);
                _env.WebRootPath = webRoot;
            }

            var logsPath = Path.Combine(webRoot, "uploads", "logs");
            Directory.CreateDirectory(logsPath);
            return logsPath;
        }

        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            var uploadsRoot = EnsureUploadsDirectory();
            var fileName = Path.GetFileName(imageUrl);
            var absolutePath = Path.Combine(uploadsRoot, fileName);
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
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

        private bool IsSocietyAccount()
        {
            var accountType = User.FindFirstValue("account_type");
            return Enum.TryParse<AccountType>(accountType, true, out var parsed) && parsed == AccountType.Society;
        }
    }
}
