using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtlProject.Data.Entities.Dwh.Dimensions
{
    [Table("Dim_Tiempo", Schema = "dbo")]
    public class Dim_Tiempo
    {
        [Key]
        public int IdFecha { get; set; }
        public DateTime Fecha { get; set; }
        [Column("Año")]
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string? NombreMes { get; set; }
        public int Dia { get; set; }
        public int Trimestre { get; set; }
        public int DiaSemana { get; set; }
    }
}
