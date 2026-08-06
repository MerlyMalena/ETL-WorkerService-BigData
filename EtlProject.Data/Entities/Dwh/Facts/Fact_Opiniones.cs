using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Facts
{
    [Table("Fact_Opiniones", Schema = "dbo")]
    public class Fact_Opiniones
    {
        [Key]
        public int IdOpinion { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int IdFuente { get; set; }
        public int IdClasificacion { get; set; }
        public int IdFecha { get; set; }
        
        public int? PuntajeSatisfaccion { get; set; }
        public decimal? Rating { get; set; }
        public string? Comentario { get; set; }
        public int? Cantidad { get; set; }
    }
}
