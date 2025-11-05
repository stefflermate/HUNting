using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using HUNting.API.Models;
using HUNting.API.Models.Ads;
using HUNting.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly IAdService _svc;
        private readonly IWebHostEnvironment _env;

        public AdsController(IAdService svc, IWebHostEnvironment env)
        {
            _svc = svc;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll([FromQuery] AdQueryOptions options)
        {
            var result = await _svc.GetPagedAsync(options);
            return Ok(result);
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AdCategory>>> Categories()
            => Ok(await _svc.GetCategoriesAsync());

        [HttpGet("statuses")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AdStatus>>> Statuses()
            => Ok(await _svc.GetStatusesAsync());

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Ad>> Get(int id)
        {
            var ad = await _svc.GetByIdAsync(id);
            return ad == null ? NotFound() : Ok(ad);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Ad>> Create([FromBody] Ad ad)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin)
            {
                ad.OwnerUserId = userId.Value;
                ad.OwnerSocietyId = null;
            }
            else if (!ad.OwnerUserId.HasValue && !ad.OwnerSocietyId.HasValue)
            {
                ad.OwnerUserId = userId.Value;
            }

            try
            {
                await _svc.AddAsync(ad);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction(nameof(Get), new { id = ad.AdId }, ad);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] Ad ad)
        {
            if (id != ad.AdId) return BadRequest();

            var existing = await _svc.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && existing.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            existing.Title = ad.Title;
            existing.Description = ad.Description;
            existing.CategoryId = ad.CategoryId;
            existing.Price = ad.Price;
            existing.LocationText = ad.LocationText;
            existing.Latitude = ad.Latitude;
            existing.Longitude = ad.Longitude;
            existing.StatusCode = ad.StatusCode;
            existing.ContactEmail = ad.ContactEmail;
            existing.ContactPhone = ad.ContactPhone;

            if (isAdmin)
            {
                existing.OwnerUserId = ad.OwnerUserId;
                existing.OwnerSocietyId = ad.OwnerSocietyId;
            }

            try
            {
                await _svc.UpdateAsync(existing);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var ad = await _svc.GetByIdAsync(id);
            if (ad == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && ad.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            await _svc.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/images")]
        [Authorize]
        [RequestSizeLimit(10_485_760)]
        public async Task<ActionResult<AdImage>> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nem talalhato feltoltendo fajl.");
            }

            var ad = await _svc.GetByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && ad.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(extension))
            {
                return BadRequest("Csak JPG, JPEG, PNG vagy WEBP fajl tamogatott.");
            }

            var rootPath = EnsureUploadsDirectory();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var relativePath = Path.Combine("uploads", "ads", fileName).Replace("\\", "/");
            var absolutePath = Path.Combine(rootPath, fileName);

            await using (var stream = System.IO.File.Create(absolutePath))
            {
                await file.CopyToAsync(stream);
            }

            var image = await _svc.AddImageAsync(id, "/" + relativePath);

            return Created(image.ImageUrl, image);
        }

        [HttpDelete("{id:int}/images/{imageId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var ad = await _svc.GetByIdAsync(id);
            if (ad == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && ad.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            var image = await _svc.GetImageByIdAsync(imageId);
            if (image == null || image.AdId != id)
            {
                return NotFound();
            }

            var rootUploads = GetUploadsRoot();
            if (rootUploads != null)
            {
                var fileName = Path.GetFileName(image.ImageUrl);
                var absolutePath = Path.Combine(rootUploads, fileName);
                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
            }

            await _svc.RemoveImageAsync(image);
            return NoContent();
        }

        private int? GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out var id) ? id : null;
        }

        private string EnsureUploadsDirectory()
        {
            var root = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                Directory.CreateDirectory(root);
                _env.WebRootPath = root;
            }

            var adsPath = Path.Combine(root, "uploads", "ads");
            Directory.CreateDirectory(adsPath);
            return adsPath;
        }

        private string? GetUploadsRoot()
        {
            var root = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            }

            var adsPath = Path.Combine(root, "uploads", "ads");
            return Directory.Exists(adsPath) ? adsPath : null;
        }
    }
}
