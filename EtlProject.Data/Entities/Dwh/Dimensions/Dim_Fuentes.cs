using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Fuentes", Schema = "dbo")]
    public class Dim_Fuentes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdFuente { get; set; }
        public string? NombreFuente { get; set; }
        public string? TipoFuente { get; set; }
    }
}
