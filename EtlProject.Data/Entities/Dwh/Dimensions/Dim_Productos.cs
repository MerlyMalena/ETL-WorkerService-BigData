using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Productos", Schema = "dbo")]
    public class Dim_Productos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdProducto { get; set; }
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
    }
}
