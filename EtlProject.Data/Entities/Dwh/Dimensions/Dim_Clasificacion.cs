using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Clasificacion", Schema = "dwh")]
    public class Dim_Clasificacion
    {
        [Key]
        public int ClasificacionKey { get; set; }
        public decimal RangoInicio { get; set; }
        public decimal RangoFin { get; set; }
        public string? Categoria { get; set; } // Positivo, Neutral, Negativo
    }
}
