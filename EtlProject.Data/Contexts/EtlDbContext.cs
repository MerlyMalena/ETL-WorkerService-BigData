using EtlProject.Data.Entities.Dwh.Dimensions;
using EtlProject.Data.Entities.Dwh.Facts;
using EtlProject.Data.Entities.Staging;
using Microsoft.EntityFrameworkCore;

namespace EtlProject.Data.Contexts
{
    public class EtlDbContext : DbContext
    {
        public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
        {
        }

        // Staging
        public DbSet<ReviewStaging> ReviewStaging { get; set; }

        // DWH Dimensions
        public DbSet<Dim_Clientes> Dim_Clientes { get; set; }
        public DbSet<Dim_Productos> Dim_Productos { get; set; }
        public DbSet<Dim_Fuentes> Dim_Fuentes { get; set; }
        public DbSet<Dim_Clasificacion> Dim_Clasificacion { get; set; }
        public DbSet<Dim_Tiempo> Dim_Tiempo { get; set; }

        // DWH Facts
        public DbSet<Fact_Opiniones> Fact_Opiniones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Fix Decimal Warnings
            modelBuilder.Entity<Fact_Opiniones>().Property(f => f.Rating).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ReviewStaging>().Property(r => r.Rating).HasColumnType("decimal(18,2)");
            // Navigation properties removed for simpler FK mapping
        }
    }
}
