using EtlProject.Data.Contexts;
using EtlProject.Data.Entities.Dwh.Facts;
using EtlProject.Worker.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtlProject.Worker.Services
{
    public class FactLoaderService : IFactLoader
    {
        private readonly EtlDbContext _dbContext;
        private readonly ILogger<FactLoaderService> _logger;

        public FactLoaderService(EtlDbContext dbContext, ILogger<FactLoaderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task LoadFactsAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando Carga de Hechos (Fact_Opiniones) desde Staging...");

                // 1. Limpieza de idempotencia: Vaciamos la tabla de hechos antes de insertar
                _logger.LogInformation("Vaciando tabla de hechos Fact_Opiniones...");
                await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.Fact_Opiniones;");

                // 2. Extraer todos los registros de Staging
                var stagingRecords = await _dbContext.ReviewStaging.ToListAsync();

                if (!stagingRecords.Any())
                {
                    _logger.LogWarning("No hay registros en Staging para cargar a Fact_Opiniones.");
                    return;
                }

                // 3. Mapeo y Transformación
                var factRecords = stagingRecords.Select(s => 
                {
                    // Parseo del ID de Opinión
                    int idOpinion = 0;
                    if (!string.IsNullOrEmpty(s.ReviewId))
                    {
                        string cleanReviewId = new string(s.ReviewId.Where(char.IsDigit).ToArray());
                        int.TryParse(cleanReviewId, out idOpinion);
                    }
                    if (idOpinion == 0) idOpinion = s.Id; // Fallback al ID de staging si viene vacío

                    // Parseo de Cliente
                    int idCliente = 0;
                    if (!string.IsNullOrEmpty(s.ClientId))
                    {
                        string cleanClientId = new string(s.ClientId.Where(char.IsDigit).ToArray());
                        int.TryParse(cleanClientId, out idCliente);
                    }

                    // Parseo de Producto
                    int idProducto = 0;
                    if (!string.IsNullOrEmpty(s.ProductId))
                    {
                        string cleanProductId = new string(s.ProductId.Where(char.IsDigit).ToArray());
                        int.TryParse(cleanProductId, out idProducto);
                    }

                    // Parseo de Fecha
                    int idFecha = 0;
                    if (s.ReviewDate.HasValue)
                    {
                        idFecha = int.Parse(s.ReviewDate.Value.ToString("yyyyMMdd"));
                    }

                    // Clasificación de la Fuente
                    int idFuente = s.SourceType == "CSV" ? 1 : s.SourceType == "API" ? 2 : s.SourceType == "DB" ? 3 : 99;

                    // Clasificación del Sentimiento (Rating)
                    int idClasificacion = 2; // Neutra por defecto
                    if (s.Rating.HasValue)
                    {
                        if (s.Rating.Value < 3) idClasificacion = 1; // Negativa
                        else if (s.Rating.Value >= 4) idClasificacion = 3; // Positiva
                    }

                    return new Fact_Opiniones
                    {
                        IdOpinion = idOpinion,
                        IdCliente = idCliente,
                        IdProducto = idProducto,
                        IdFuente = idFuente,
                        IdClasificacion = idClasificacion,
                        IdFecha = idFecha,
                        PuntajeSatisfaccion = s.Rating.HasValue ? (int)s.Rating.Value : null,
                        Rating = s.Rating,
                        Comentario = s.Comment,
                        Cantidad = 1
                    };
                }).ToList();

                // 4. Inserción
                // Eliminamos duplicados potenciales por clave primaria usando GroupBy
                var factRecordsUnicos = factRecords
                    .GroupBy(f => f.IdOpinion)
                    .Select(g => g.First())
                    .ToList();

                await _dbContext.Fact_Opiniones.AddRangeAsync(factRecordsUnicos);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Carga de Hechos finalizada. {Count} registros insertados en Fact_Opiniones.", factRecordsUnicos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la carga de la tabla de hechos Fact_Opiniones.");
            }
        }
    }
}
