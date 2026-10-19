using System;
using System.Linq;
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
    public class AdsController : ApiControllerBase
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

            var isAdmin = IsAdministrator;
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

            var isAdmin = IsAdministrator;
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

            if (!IsAdministrator && ad.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            await _svc.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/images")]
        [Authorize]
        [RequestSizeLimit(10_485_760)]
        public async Task<ActionResult<AdImage>> UploadImage(int id, [FromForm] IFormFile file)
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

            if (!IsAdministrator && ad.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            AdImage image;
            try
            {
                var imageUrl = await UploadStorage.SaveImageAsync(_env, "ads", file);
                image = await _svc.AddImageAsync(id, imageUrl);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

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

            if (!IsAdministrator && ad.OwnerUserId != userId.Value)
            {
                return Forbid();
            }

            var image = await _svc.GetImageByIdAsync(imageId);
            if (image == null || image.AdId != id)
            {
                return NotFound();
            }

            UploadStorage.DeleteImage(_env, "ads", image.ImageUrl);

            await _svc.RemoveImageAsync(image);
            return NoContent();
        }
    }
}
