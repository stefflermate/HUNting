using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HUNting.API.Models.Map
{
    public class MapPathPoint
    {
        [Key] public int PathPointId { get; set; }

        public int PathId { get; set; }
        public MapPath? Path { get; set; }

        public int Seq { get; set; }

        [Column(TypeName = "decimal(9,6)")] public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal Longitude { get; set; }
    }
}

