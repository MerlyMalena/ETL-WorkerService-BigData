using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Tiempo", Schema = "dwh")]
    public class Dim_Tiempo
    {
        [Key]
        public int TiempoKey { get; set; }
        public DateTime Fecha { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Dia { get; set; }
        public int Trimestre { get; set; }
        public int DiaSemana { get; set; }
    }
}
