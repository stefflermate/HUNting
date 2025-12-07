using HUNting.API.Data;
using HUNting.API.Models.Map;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HUNting.API.Services
{
    public class MapService : IMapService
    {
        private readonly HuntingContext _context;

        public MapService(HuntingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MapMarker>> GetMarkersAsync(int? ownerUserId, int? societyId, CancellationToken cancellationToken = default)
        {
            var query = _context.MapMarkers
                .AsNoTracking()
                .Include(m => m.IconType)
                .Include(m => m.Visibility)
                .AsQueryable();

            if (ownerUserId.HasValue)
            {
                query = query.Where(m => m.SocietyId == null && m.CreatedByUserId == ownerUserId.Value);
            }
            else if (societyId.HasValue)
            {
                query = query.Where(m => m.SocietyId == societyId.Value);
            }
            else
            {
                return Enumerable.Empty<MapMarker>();
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<MapMarker> CreateMarkerAsync(MapMarker marker, CancellationToken cancellationToken = default)
        {
            await _context.MapMarkers.AddAsync(marker, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return marker;
        }

        public async Task<MapMarker?> UpdateMarkerAsync(MapMarker marker, int currentUserId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapMarkers.FirstOrDefaultAsync(m => m.MarkerId == marker.MarkerId, cancellationToken);
            if (existing == null) return null;
            if (!CanModify(existing.CreatedByUserId, existing.SocietyId, currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            existing.IconTypeId = marker.IconTypeId;
            existing.Latitude = marker.Latitude;
            existing.Longitude = marker.Longitude;
            existing.Note = marker.Note;
            existing.VisibilityCode = marker.VisibilityCode;
            existing.SocietyId = marker.SocietyId;

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task DeleteMarkerAsync(int markerId, int currentUserId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapMarkers.FindAsync(new object?[] { markerId }, cancellationToken);
            if (existing == null) return;
            if (!CanModify(existing.CreatedByUserId, existing.SocietyId, currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            _context.MapMarkers.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<MapPolygon>> GetPolygonsAsync(int? ownerUserId, int? societyId, CancellationToken cancellationToken = default)
        {
            var query = _context.MapPolygons
                .AsNoTracking()
                .Include(p => p.Points)
                .Include(p => p.Visibility)
                .AsQueryable();

            if (ownerUserId.HasValue)
            {
                query = query.Where(p => p.SocietyId == null && p.CreatedByUserId == ownerUserId.Value);
            }
            else if (societyId.HasValue)
            {
                query = query.Where(p => p.SocietyId == societyId.Value);
            }
            else
            {
                return Enumerable.Empty<MapPolygon>();
            }

            var polygons = await query.ToListAsync(cancellationToken);

            foreach (var polygon in polygons)
            {
                polygon.Points = polygon.Points
                    .OrderBy(pp => pp.Seq)
                    .ToList();
            }

            return polygons;
        }

        public async Task<MapPolygon> CreatePolygonAsync(MapPolygon polygon, CancellationToken cancellationToken = default)
        {
            await _context.MapPolygons.AddAsync(polygon, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return polygon;
        }

        public async Task<MapPolygon?> UpdatePolygonAsync(MapPolygon polygon, int currentUserId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapPolygons
                .Include(p => p.Points)
                .FirstOrDefaultAsync(p => p.PolygonId == polygon.PolygonId, cancellationToken);
            if (existing == null) return null;
            if (!CanModify(existing.CreatedByUserId, existing.SocietyId, currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            existing.Name = polygon.Name;
            existing.VisibilityCode = polygon.VisibilityCode;
            existing.StrokeColor = polygon.StrokeColor;
            existing.FillColor = polygon.FillColor;
            existing.SocietyId = polygon.SocietyId;

            _context.MapPolygonPoints.RemoveRange(existing.Points);
            existing.Points = polygon.Points
                .Select(pp => new MapPolygonPoint
                {
                    Seq = pp.Seq,
                    Latitude = pp.Latitude,
                    Longitude = pp.Longitude
                })
                .ToList();

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task DeletePolygonAsync(int polygonId, int currentUserId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapPolygons.FindAsync(new object?[] { polygonId }, cancellationToken);
            if (existing == null) return;
            if (!CanModify(existing.CreatedByUserId, existing.SocietyId, currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            _context.MapPolygons.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<MapIconType>> GetIconTypesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MapIconTypes.AsNoTracking().OrderBy(i => i.Name).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MapVisibility>> GetVisibilitiesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MapVisibilities.AsNoTracking().OrderBy(v => v.VisibilityCode).ToListAsync(cancellationToken);
        }

        public Task<bool> UserBelongsToSocietyAsync(int societyId, int userId, CancellationToken cancellationToken = default)
        {
            return _context.SocietyMembers.AnyAsync(m => m.SocietyId == societyId && m.UserId == userId, cancellationToken);
        }

        private static bool CanModify(int creatorUserId, int? societyId, int currentUserId)
        {
            if (societyId == null)
            {
                return creatorUserId == currentUserId;
            }

            // For society-owned items, allow modification if creator == current user.
            // Additional checks (e.g., roles) can be added later.
            return creatorUserId == currentUserId;
        }
    }
}
