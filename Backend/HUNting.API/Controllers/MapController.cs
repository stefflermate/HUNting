using System.Linq;
using HUNting.API.Models.Map;
using HUNting.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ApiControllerBase
    {
        private readonly IMapService _service;

        public MapController(IMapService service)
        {
            _service = service;
        }

        [HttpGet("config")]
        public async Task<ActionResult> GetConfig(CancellationToken cancellationToken)
        {
            var icons = await _service.GetIconTypesAsync(cancellationToken);
            var visibilities = await _service.GetVisibilitiesAsync(cancellationToken);

            return Ok(new
            {
                iconTypes = icons.Select(i => new
                {
                    i.IconTypeId,
                    i.Name,
                    i.DefaultColor,
                    i.Glyph
                }),
                visibilities = visibilities.Select(v => new
                {
                    v.VisibilityCode,
                    v.Description
                })
            });
        }

        [HttpGet("markers")]
        public async Task<ActionResult<IEnumerable<MapMarkerResponse>>> GetMarkers([FromQuery] MapLayerQuery query, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var scope = MapLayerScope.From(query.Scope);

            if (scope == MapLayerScope.Personal)
            {
                if (userId == null) return Unauthorized();
                var items = await _service.GetMarkersAsync(userId.Value, null, cancellationToken);
                return Ok(items.Select(MapMarkerResponse.FromEntity));
            }

            if (scope == MapLayerScope.Society)
            {
                if (userId == null) return Unauthorized();
                if (query.SocietyId == null) return BadRequest("societyId is required.");

                var canAccess = await _service.UserBelongsToSocietyAsync(query.SocietyId.Value, userId.Value, cancellationToken);
                if (!canAccess) return Forbid();

                var items = await _service.GetMarkersAsync(null, query.SocietyId, cancellationToken);
                return Ok(items.Select(MapMarkerResponse.FromEntity));
            }

            return Ok(Enumerable.Empty<MapMarkerResponse>());
        }

        [HttpPost("markers")]
        [Authorize]
        public async Task<ActionResult<MapMarkerResponse>> CreateMarker([FromBody] MapMarkerRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (request.SocietyId.HasValue)
            {
                var allowed = await _service.UserBelongsToSocietyAsync(request.SocietyId.Value, userId.Value, cancellationToken);
                if (!allowed) return Forbid();
            }

            var entity = new MapMarker
            {
                CreatedByUserId = userId.Value,
                IconTypeId = request.IconTypeId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Note = request.Note,
                VisibilityCode = request.VisibilityCode ?? "PUBLIC",
                SocietyId = request.SocietyId
            };

            var created = await _service.CreateMarkerAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(GetMarkers), MapMarkerResponse.FromEntity(created));
        }

        [HttpPut("markers/{markerId:int}")]
        [Authorize]
        public async Task<ActionResult<MapMarkerResponse>> UpdateMarker(int markerId, [FromBody] MapMarkerRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (request.SocietyId.HasValue)
            {
                var allowed = await _service.UserBelongsToSocietyAsync(request.SocietyId.Value, userId.Value, cancellationToken);
                if (!allowed) return Forbid();
            }

            var entity = new MapMarker
            {
                MarkerId = markerId,
                CreatedByUserId = userId.Value,
                IconTypeId = request.IconTypeId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Note = request.Note,
                VisibilityCode = request.VisibilityCode ?? "PUBLIC",
                SocietyId = request.SocietyId
            };

            MapMarker? updated;
            try
            {
                updated = await _service.UpdateMarkerAsync(entity, userId.Value, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return updated == null ? NotFound() : Ok(MapMarkerResponse.FromEntity(updated));
        }

        [HttpDelete("markers/{markerId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteMarker(int markerId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _service.DeleteMarkerAsync(markerId, userId.Value, cancellationToken);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("polygons")]
        public async Task<ActionResult<IEnumerable<MapPolygonResponse>>> GetPolygons([FromQuery] MapLayerQuery query, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var scope = MapLayerScope.From(query.Scope);

            if (scope == MapLayerScope.Personal)
            {
                if (userId == null) return Unauthorized();
                var items = await _service.GetPolygonsAsync(userId.Value, null, cancellationToken);
                return Ok(items.Select(MapPolygonResponse.FromEntity));
            }

            if (scope == MapLayerScope.Society)
            {
                if (userId == null) return Unauthorized();
                if (query.SocietyId == null) return BadRequest("societyId is required.");

                var allowed = await _service.UserBelongsToSocietyAsync(query.SocietyId.Value, userId.Value, cancellationToken);
                if (!allowed) return Forbid();

                var items = await _service.GetPolygonsAsync(null, query.SocietyId, cancellationToken);
                return Ok(items.Select(MapPolygonResponse.FromEntity));
            }

            return Ok(Enumerable.Empty<MapPolygonResponse>());
        }

        [HttpPost("polygons")]
        [Authorize]
        public async Task<ActionResult<MapPolygonResponse>> CreatePolygon([FromBody] MapPolygonRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (request.SocietyId.HasValue)
            {
                var allowed = await _service.UserBelongsToSocietyAsync(request.SocietyId.Value, userId.Value, cancellationToken);
                if (!allowed) return Forbid();
            }

            var polygon = new MapPolygon
            {
                CreatedByUserId = userId.Value,
                Name = request.Name,
                Description = request.Description,
                VisibilityCode = request.VisibilityCode ?? "PUBLIC",
                StrokeColor = request.StrokeColor,
                FillColor = request.FillColor,
                SocietyId = request.SocietyId,
                Points = request.Points
                    .Select((p, index) => new MapPolygonPoint
                    {
                        Seq = index,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude
                    })
                    .ToList()
            };

            var created = await _service.CreatePolygonAsync(polygon, cancellationToken);
            return CreatedAtAction(nameof(GetPolygons), MapPolygonResponse.FromEntity(created));
        }

        [HttpPut("polygons/{polygonId:int}")]
        [Authorize]
        public async Task<ActionResult<MapPolygonResponse>> UpdatePolygon(int polygonId, [FromBody] MapPolygonRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (request.SocietyId.HasValue)
            {
                var allowed = await _service.UserBelongsToSocietyAsync(request.SocietyId.Value, userId.Value, cancellationToken);
                if (!allowed) return Forbid();
            }

            var polygon = new MapPolygon
            {
                PolygonId = polygonId,
                CreatedByUserId = userId.Value,
                Name = request.Name,
                Description = request.Description,
                VisibilityCode = request.VisibilityCode ?? "PUBLIC",
                StrokeColor = request.StrokeColor,
                FillColor = request.FillColor,
                SocietyId = request.SocietyId,
                Points = request.Points
                    .Select((p, index) => new MapPolygonPoint
                    {
                        Seq = index,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude
                    })
                    .ToList()
            };

            MapPolygon? updated;
            try
            {
                updated = await _service.UpdatePolygonAsync(polygon, userId.Value, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return updated == null ? NotFound() : Ok(MapPolygonResponse.FromEntity(updated));
        }

        [HttpDelete("polygons/{polygonId:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePolygon(int polygonId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _service.DeletePolygonAsync(polygonId, userId.Value, cancellationToken);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        public record MapLayerQuery(string? Scope, int? SocietyId);

        public record MapMarkerRequest(int IconTypeId, decimal Latitude, decimal Longitude, string? Note, string? VisibilityCode, int? SocietyId);

        public record MapMarkerResponse(int MarkerId, int IconTypeId, decimal Latitude, decimal Longitude, string? Note, string VisibilityCode, int? SocietyId)
        {
            public static MapMarkerResponse FromEntity(MapMarker marker) =>
                new(marker.MarkerId, marker.IconTypeId, marker.Latitude, marker.Longitude, marker.Note, marker.VisibilityCode, marker.SocietyId);
        }

        public record MapPointRequest(decimal Latitude, decimal Longitude);

        public record MapPolygonRequest(string? Name, string? Description, string? VisibilityCode, string? StrokeColor, string? FillColor, int? SocietyId, IList<MapPointRequest> Points);

        public record MapPolygonResponse(int PolygonId, string? Name, string? Description, string VisibilityCode, string? StrokeColor, string? FillColor, int? SocietyId, IEnumerable<MapPointResponse> Points)
        {
            public static MapPolygonResponse FromEntity(MapPolygon polygon) =>
                new(
                    polygon.PolygonId,
                    polygon.Name,
                    polygon.Description,
                    polygon.VisibilityCode,
                    polygon.StrokeColor,
                    polygon.FillColor,
                    polygon.SocietyId,
                    polygon.Points.OrderBy(p => p.Seq).Select(MapPointResponse.FromEntity));
        }

        public record MapPointResponse(int Seq, decimal Latitude, decimal Longitude)
        {
            public static MapPointResponse FromEntity(MapPolygonPoint point) =>
                new(point.Seq, point.Latitude, point.Longitude);
        }
    }

    public readonly record struct MapLayerScope(string Value)
    {
        public static MapLayerScope None => new("none");
        public static MapLayerScope Personal => new("personal");
        public static MapLayerScope Society => new("society");

        public static MapLayerScope From(string? raw) =>
            raw?.ToLowerInvariant() switch
            {
                "personal" => Personal,
                "society" => Society,
                _ => None
            };
    }
}
