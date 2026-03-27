using HUNting.API.Data;
using HUNting.API.Models;
using HUNting.API.Models.Ads;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Services
{
    public class AdService : IAdService
    {
        private readonly HuntingContext _context;

        public AdService(HuntingContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Ad>> GetPagedAsync(AdQueryOptions options)
        {
            var page = options.Page < 1 ? 1 : options.Page;
            var take = options.Take is < 1 or > 100 ? 12 : options.Take;

            IQueryable<Ad> query = _context.Ads
                .AsNoTracking()
                .Include(a => a.Category)
                .Include(a => a.Images)
                .Include(a => a.Status)
                .Include(a => a.OwnerUser)
                .Include(a => a.OwnerSociety);

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var term = options.Search.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Title, $"%{term}%") ||
                    (a.Description != null && EF.Functions.Like(a.Description, $"%{term}%")));
            }

            if (options.CategoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == options.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.StatusCode))
            {
                query = query.Where(a => a.StatusCode == options.StatusCode);
            }

            if (options.MinPrice.HasValue)
            {
                query = query.Where(a => a.Price >= options.MinPrice.Value);
            }

            if (options.MaxPrice.HasValue)
            {
                query = query.Where(a => a.Price <= options.MaxPrice.Value);
            }

            if (options.OwnerUserId.HasValue)
            {
                query = query.Where(a => a.OwnerUserId == options.OwnerUserId.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * take)
                .Take(take)
                .ToListAsync();

            return new PagedResult<Ad>(items, total, page, take);
        }

        public async Task<Ad?> GetByIdAsync(int id)
        {
            return await _context.Ads
                .Include(a => a.Category)
                .Include(a => a.Images)
                .Include(a => a.Status)
                .Include(a => a.OwnerUser)
                .Include(a => a.OwnerSociety)
                .FirstOrDefaultAsync(a => a.AdId == id);
        }

        public async Task<IEnumerable<AdCategory>> GetCategoriesAsync()
        {
            return await _context.AdCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdStatus>> GetStatusesAsync()
        {
            return await _context.AdStatuses
                .AsNoTracking()
                .OrderBy(s => s.StatusCode)
                .ToListAsync();
        }

        public async Task AddAsync(Ad ad)
        {
            EnsureSingleOwner(ad);
            ad.CreatedAt = DateTime.UtcNow;

            await _context.Ads.AddAsync(ad);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Ad ad)
        {
            EnsureSingleOwner(ad);

            _context.Ads.Update(ad);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Ads.FindAsync(id);
            if (entity == null) return;

            _context.Ads.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<AdImage> AddImageAsync(int adId, string imageUrl, int? sortOrder = null)
        {
            var exists = await _context.Ads.AnyAsync(a => a.AdId == adId);
            if (!exists)
            {
                throw new ArgumentException("A megadott hirdetes nem talalhato.");
            }

            var nextOrder = sortOrder ??
                ((await _context.AdImages
                    .Where(i => i.AdId == adId)
                    .Select(i => (int?)i.SortOrder)
                    .MaxAsync()) ?? 0) + 1;

            var image = new AdImage
            {
                AdId = adId,
                ImageUrl = imageUrl,
                SortOrder = nextOrder
            };

            _context.AdImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<AdImage?> GetImageByIdAsync(int imageId)
        {
            return await _context.AdImages.FirstOrDefaultAsync(i => i.ImageId == imageId);
        }

        public async Task RemoveImageAsync(AdImage image)
        {
            _context.AdImages.Remove(image);
            await _context.SaveChangesAsync();
        }

        private static void EnsureSingleOwner(Ad ad)
        {
            var hasUser = ad.OwnerUserId.HasValue;
            var hasSociety = ad.OwnerSocietyId.HasValue;
            if (hasUser == hasSociety)
            {
                throw new ArgumentException("Egy hirdetéshez pontosan egy tulajdonost kell megadni (felhasználó vagy társaság).");
            }
        }
    }
}
