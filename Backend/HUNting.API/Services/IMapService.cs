using HUNting.API.Models.Map;

namespace HUNting.API.Services
{
    public interface IMapService
    {
        Task<IEnumerable<MapMarker>> GetMarkersAsync(int? ownerUserId, int? societyId, CancellationToken cancellationToken = default);
        Task<MapMarker> CreateMarkerAsync(MapMarker marker, CancellationToken cancellationToken = default);
        Task<MapMarker?> UpdateMarkerAsync(MapMarker marker, int currentUserId, CancellationToken cancellationToken = default);
        Task DeleteMarkerAsync(int markerId, int currentUserId, CancellationToken cancellationToken = default);

        Task<IEnumerable<MapPolygon>> GetPolygonsAsync(int? ownerUserId, int? societyId, CancellationToken cancellationToken = default);
        Task<MapPolygon> CreatePolygonAsync(MapPolygon polygon, CancellationToken cancellationToken = default);
        Task<MapPolygon?> UpdatePolygonAsync(MapPolygon polygon, int currentUserId, CancellationToken cancellationToken = default);
        Task DeletePolygonAsync(int polygonId, int currentUserId, CancellationToken cancellationToken = default);

        Task<IEnumerable<MapIconType>> GetIconTypesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MapVisibility>> GetVisibilitiesAsync(CancellationToken cancellationToken = default);
        Task<bool> UserBelongsToSocietyAsync(int societyId, int userId, CancellationToken cancellationToken = default);
    }
}
