using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Productos", Schema = "dwh")]
    public class Dim_Productos
    {
        [Key]
        public int ProductoKey { get; set; }
        public string? IdOriginal { get; set; }
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public decimal? Precio { get; set; }
    }
}
