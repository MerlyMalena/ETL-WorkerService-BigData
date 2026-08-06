using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Clientes", Schema = "dbo")]
    public class Dim_Clientes
    {
        [Key]
        public int ClienteKey { get; set; }
        public string? IdOriginal { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Ubicacion { get; set; }
    }
}
