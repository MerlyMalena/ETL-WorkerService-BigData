using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Clasificacion", Schema = "dbo")]
    public class Dim_Clasificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdClasificacion { get; set; }
        public string? NombreClasificacion { get; set; }
    }
}
