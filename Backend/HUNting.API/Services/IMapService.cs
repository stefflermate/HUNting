using HUNting.API.Models.Map;

namespace HUNting.API.Services
{
    public interface IMapService
    {
        Task<IEnumerable<MapMarker>> GetMarkersAsync(CancellationToken cancellationToken = default);
        Task<MapMarker> CreateMarkerAsync(MapMarker marker, CancellationToken cancellationToken = default);
        Task<MapMarker?> UpdateMarkerAsync(MapMarker marker, CancellationToken cancellationToken = default);
        Task DeleteMarkerAsync(int markerId, CancellationToken cancellationToken = default);

        Task<IEnumerable<MapPolygon>> GetPolygonsAsync(CancellationToken cancellationToken = default);
        Task<MapPolygon> CreatePolygonAsync(MapPolygon polygon, CancellationToken cancellationToken = default);
        Task<MapPolygon?> UpdatePolygonAsync(MapPolygon polygon, CancellationToken cancellationToken = default);
        Task DeletePolygonAsync(int polygonId, CancellationToken cancellationToken = default);

        Task<IEnumerable<MapPath>> GetPathsAsync(CancellationToken cancellationToken = default);
        Task<MapPath> CreatePathAsync(MapPath path, CancellationToken cancellationToken = default);
        Task<MapPath?> UpdatePathAsync(MapPath path, CancellationToken cancellationToken = default);
        Task DeletePathAsync(int pathId, CancellationToken cancellationToken = default);

        Task<IEnumerable<MapIconType>> GetIconTypesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MapVisibility>> GetVisibilitiesAsync(CancellationToken cancellationToken = default);
    }
}

