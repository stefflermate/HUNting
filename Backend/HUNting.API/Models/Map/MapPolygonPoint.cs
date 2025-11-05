using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.Map
{
    public class MapPolygonPoint
    {
        [Key] public int PolygonPointId { get; set; }

        public int PolygonId { get; set; }
        public MapPolygon? Polygon { get; set; }

        public int Seq { get; set; }

        [Column(TypeName = "decimal(9,6)")] public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal Longitude { get; set; }
    }
}

