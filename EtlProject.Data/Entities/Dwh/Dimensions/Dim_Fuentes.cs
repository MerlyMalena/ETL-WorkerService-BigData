using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Fuentes", Schema = "dbo")]
    public class Dim_Fuentes
    {
        [Key]
        public int FuenteKey { get; set; }
        public string? TipoFuente { get; set; } // SV, Relational DB, REST API
        public string? Descripcion { get; set; }
    }
}
