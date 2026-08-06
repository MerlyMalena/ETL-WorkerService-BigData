using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Clientes", Schema = "dbo")]
    public class Dim_Clientes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
    }
}
