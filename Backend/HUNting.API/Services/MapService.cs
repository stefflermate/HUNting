using HUNting.API.Data;
using HUNting.API.Models.Map;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Services
{
    public class MapService : IMapService
    {
        private readonly HuntingContext _context;

        public MapService(HuntingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MapMarker>> GetMarkersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MapMarkers
                .AsNoTracking()
                .Include(m => m.IconType)
                .Include(m => m.Visibility)
                .ToListAsync(cancellationToken);
        }

        public async Task<MapMarker> CreateMarkerAsync(MapMarker marker, CancellationToken cancellationToken = default)
        {
            await _context.MapMarkers.AddAsync(marker, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return marker;
        }

        public async Task<MapMarker?> UpdateMarkerAsync(MapMarker marker, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapMarkers.FirstOrDefaultAsync(m => m.MarkerId == marker.MarkerId, cancellationToken);
            if (existing == null) return null;

            existing.IconTypeId = marker.IconTypeId;
            existing.Latitude = marker.Latitude;
            existing.Longitude = marker.Longitude;
            existing.Note = marker.Note;
            existing.VisibilityCode = marker.VisibilityCode;
            existing.SocietyId = marker.SocietyId;

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task DeleteMarkerAsync(int markerId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapMarkers.FindAsync(new object?[] { markerId }, cancellationToken);
            if (existing == null) return;

            _context.MapMarkers.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<MapPolygon>> GetPolygonsAsync(CancellationToken cancellationToken = default)
        {
            var polygons = await _context.MapPolygons
                .AsNoTracking()
                .Include(p => p.Points)
                .Include(p => p.Visibility)
                .ToListAsync(cancellationToken);

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

        public async Task<MapPolygon?> UpdatePolygonAsync(MapPolygon polygon, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapPolygons
                .Include(p => p.Points)
                .FirstOrDefaultAsync(p => p.PolygonId == polygon.PolygonId, cancellationToken);
            if (existing == null) return null;

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

        public async Task DeletePolygonAsync(int polygonId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapPolygons.FindAsync(new object?[] { polygonId }, cancellationToken);
            if (existing == null) return;

            _context.MapPolygons.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<MapPath>> GetPathsAsync(CancellationToken cancellationToken = default)
        {
            var paths = await _context.MapPaths
                .AsNoTracking()
                .Include(p => p.Points)
                .Include(p => p.Visibility)
                .ToListAsync(cancellationToken);

            foreach (var path in paths)
            {
                path.Points = path.Points
                    .OrderBy(pp => pp.Seq)
                    .ToList();
            }

            return paths;
        }

        public async Task<MapPath> CreatePathAsync(MapPath path, CancellationToken cancellationToken = default)
        {
            await _context.MapPaths.AddAsync(path, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return path;
        }

        public async Task<MapPath?> UpdatePathAsync(MapPath path, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapPaths
                .Include(p => p.Points)
                .FirstOrDefaultAsync(p => p.PathId == path.PathId, cancellationToken);
            if (existing == null) return null;

            existing.Name = path.Name;
            existing.VisibilityCode = path.VisibilityCode;
            existing.StrokeColor = path.StrokeColor;
            existing.SocietyId = path.SocietyId;

            _context.MapPathPoints.RemoveRange(existing.Points);
            existing.Points = path.Points
                .Select(pp => new MapPathPoint
                {
                    Seq = pp.Seq,
                    Latitude = pp.Latitude,
                    Longitude = pp.Longitude
                })
                .ToList();

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task DeletePathAsync(int pathId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.MapPaths.FindAsync(new object?[] { pathId }, cancellationToken);
            if (existing == null) return;

            _context.MapPaths.Remove(existing);
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
    }
}
