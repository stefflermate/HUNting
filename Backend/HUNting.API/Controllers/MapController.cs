using System.Security.Claims;
using HUNting.API.Models.Map;
using HUNting.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
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
        public async Task<ActionResult<IEnumerable<MapMarkerResponse>>> GetMarkers(CancellationToken cancellationToken)
        {
            var markers = await _service.GetMarkersAsync(cancellationToken);
            return Ok(markers.Select(MapMarkerResponse.FromEntity));
        }

        [HttpPost("markers")]
        [Authorize]
        public async Task<ActionResult<MapMarkerResponse>> CreateMarker([FromBody] MapMarkerRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

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

            var updated = await _service.UpdateMarkerAsync(entity, cancellationToken);
            return updated == null ? NotFound() : Ok(MapMarkerResponse.FromEntity(updated));
        }

        [HttpDelete("markers/{markerId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteMarker(int markerId, CancellationToken cancellationToken)
        {
            await _service.DeleteMarkerAsync(markerId, cancellationToken);
            return NoContent();
        }

        [HttpGet("polygons")]
        public async Task<ActionResult<IEnumerable<MapPolygonResponse>>> GetPolygons(CancellationToken cancellationToken)
        {
            var polygons = await _service.GetPolygonsAsync(cancellationToken);
            return Ok(polygons.Select(MapPolygonResponse.FromEntity));
        }

        [HttpPost("polygons")]
        [Authorize]
        public async Task<ActionResult<MapPolygonResponse>> CreatePolygon([FromBody] MapPolygonRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var polygon = new MapPolygon
            {
                CreatedByUserId = userId.Value,
                Name = request.Name,
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

            var polygon = new MapPolygon
            {
                PolygonId = polygonId,
                CreatedByUserId = userId.Value,
                Name = request.Name,
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

            var updated = await _service.UpdatePolygonAsync(polygon, cancellationToken);
            return updated == null ? NotFound() : Ok(MapPolygonResponse.FromEntity(updated));
        }

        [HttpDelete("polygons/{polygonId:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePolygon(int polygonId, CancellationToken cancellationToken)
        {
            await _service.DeletePolygonAsync(polygonId, cancellationToken);
            return NoContent();
        }

        [HttpGet("paths")]
        public async Task<ActionResult<IEnumerable<MapPathResponse>>> GetPaths(CancellationToken cancellationToken)
        {
            var paths = await _service.GetPathsAsync(cancellationToken);
            return Ok(paths.Select(MapPathResponse.FromEntity));
        }

        [HttpPost("paths")]
        [Authorize]
        public async Task<ActionResult<MapPathResponse>> CreatePath([FromBody] MapPathRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var path = new MapPath
            {
                CreatedByUserId = userId.Value,
                Name = request.Name,
                VisibilityCode = request.VisibilityCode ?? "PUBLIC",
                StrokeColor = request.StrokeColor,
                SocietyId = request.SocietyId,
                Points = request.Points
                    .Select((p, index) => new MapPathPoint
                    {
                        Seq = index,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude
                    })
                    .ToList()
            };

            var created = await _service.CreatePathAsync(path, cancellationToken);
            return CreatedAtAction(nameof(GetPaths), MapPathResponse.FromEntity(created));
        }

        [HttpPut("paths/{pathId:int}")]
        [Authorize]
        public async Task<ActionResult<MapPathResponse>> UpdatePath(int pathId, [FromBody] MapPathRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var path = new MapPath
            {
                PathId = pathId,
                CreatedByUserId = userId.Value,
                Name = request.Name,
                VisibilityCode = request.VisibilityCode ?? "PUBLIC",
                StrokeColor = request.StrokeColor,
                SocietyId = request.SocietyId,
                Points = request.Points
                    .Select((p, index) => new MapPathPoint
                    {
                        Seq = index,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude
                    })
                    .ToList()
            };

            var updated = await _service.UpdatePathAsync(path, cancellationToken);
            return updated == null ? NotFound() : Ok(MapPathResponse.FromEntity(updated));
        }

        [HttpDelete("paths/{pathId:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePath(int pathId, CancellationToken cancellationToken)
        {
            await _service.DeletePathAsync(pathId, cancellationToken);
            return NoContent();
        }

        private int? GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var value) ? value : (int?)null;
        }

        public record MapMarkerRequest(int IconTypeId, decimal Latitude, decimal Longitude, string? Note, string? VisibilityCode, int? SocietyId);

        public record MapMarkerResponse(int MarkerId, int IconTypeId, decimal Latitude, decimal Longitude, string? Note, string VisibilityCode, int? SocietyId)
        {
            public static MapMarkerResponse FromEntity(MapMarker marker) =>
                new(marker.MarkerId, marker.IconTypeId, marker.Latitude, marker.Longitude, marker.Note, marker.VisibilityCode, marker.SocietyId);
        }

        public record MapPointRequest(decimal Latitude, decimal Longitude);

        public record MapPolygonRequest(string? Name, string? VisibilityCode, string? StrokeColor, string? FillColor, int? SocietyId, IList<MapPointRequest> Points);

        public record MapPolygonResponse(int PolygonId, string? Name, string VisibilityCode, string? StrokeColor, string? FillColor, int? SocietyId, IEnumerable<MapPointResponse> Points)
        {
            public static MapPolygonResponse FromEntity(MapPolygon polygon) =>
                new(
                    polygon.PolygonId,
                    polygon.Name,
                    polygon.VisibilityCode,
                    polygon.StrokeColor,
                    polygon.FillColor,
                    polygon.SocietyId,
                    polygon.Points.OrderBy(p => p.Seq).Select(MapPointResponse.FromEntity));
        }

        public record MapPathRequest(string? Name, string? VisibilityCode, string? StrokeColor, int? SocietyId, IList<MapPointRequest> Points);

        public record MapPathResponse(int PathId, string? Name, string VisibilityCode, string? StrokeColor, int? SocietyId, IEnumerable<MapPointResponse> Points)
        {
            public static MapPathResponse FromEntity(MapPath path) =>
                new(
                    path.PathId,
                    path.Name,
                    path.VisibilityCode,
                    path.StrokeColor,
                    path.SocietyId,
                    path.Points.OrderBy(p => p.Seq).Select(MapPointResponse.FromEntity));
        }

        public record MapPointResponse(int Seq, decimal Latitude, decimal Longitude)
        {
            public static MapPointResponse FromEntity(MapPolygonPoint point) =>
                new(point.Seq, point.Latitude, point.Longitude);

            public static MapPointResponse FromEntity(MapPathPoint point) =>
                new(point.Seq, point.Latitude, point.Longitude);
        }
    }
}

