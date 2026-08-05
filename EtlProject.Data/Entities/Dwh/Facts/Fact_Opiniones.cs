using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Facts
{
    [Table("Fact_Opiniones", Schema = "dwh")]
    public class Fact_Opiniones
    {
        [Key]
        public int OpinionKey { get; set; }
        
        // Foreign Keys
        public int ClienteKey { get; set; }
        public int ProductoKey { get; set; }
        public int FuenteKey { get; set; }
        public int TiempoKey { get; set; }
        public int ClasificacionKey { get; set; }
        
        // Measures
        public string? ComentarioOriginal { get; set; }
        public decimal RatingOriginal { get; set; }
        
        // Navigation Properties (Opcionales, pero recomendadas en EF Core)
        [ForeignKey(nameof(ClienteKey))]
        public virtual Dimensions.Dim_Clientes? Cliente { get; set; }
        
        [ForeignKey(nameof(ProductoKey))]
        public virtual Dimensions.Dim_Productos? Producto { get; set; }
        
        [ForeignKey(nameof(FuenteKey))]
        public virtual Dimensions.Dim_Fuentes? Fuente { get; set; }
        
        [ForeignKey(nameof(TiempoKey))]
        public virtual Dimensions.Dim_Tiempo? Tiempo { get; set; }
        
        [ForeignKey(nameof(ClasificacionKey))]
        public virtual Dimensions.Dim_Clasificacion? Clasificacion { get; set; }
    }
}
