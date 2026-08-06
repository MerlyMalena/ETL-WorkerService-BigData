using EtlProject.Data.Entities.Staging;
using EtlProject.Worker.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EtlProject.Worker.Extractors
{
    public class DatabaseExtractor : IExtractor
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseExtractor> _logger;

        public DatabaseExtractor(IConfiguration configuration, ILogger<DatabaseExtractor> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewStaging>> ExtractAsync()
        {
            var results = new List<ReviewStaging>();
            var connectionString = _configuration.GetConnectionString("OltpConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Cadena de conexión 'OltpConnection' no configurada.");
                return results;
            }

            try
            {
                _logger.LogInformation("Iniciando extracción desde Base de Datos Relacional (OLTP).");
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

              
                var query = @"
                    SELECT 
                        IdOpinion as ReviewId, 
                        IdCliente as ClientId, 
                        IdProducto as ProductId, 
                        Fecha as ReviewDate, 
                        Comentario as Comment, 
                        PuntajeSatisfaccion as Rating 
                    FROM web_reviews
                ";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new ReviewStaging
                    {
                        SourceType = "DB",
                        ReviewId = reader["ReviewId"]?.ToString(),
                        ClientId = reader["ClientId"]?.ToString(),
                        ProductId = reader["ProductId"]?.ToString(),
                        Comment = reader["Comment"]?.ToString(),
                        Rating = reader["Rating"] != DBNull.Value ? Convert.ToDecimal(reader["Rating"]) : null,
                        ReviewDate = reader["ReviewDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReviewDate"]) : null,
                        ExtractionDate = DateTime.UtcNow
                    });
                }
                
                _logger.LogInformation("Extracción DB completada. {Count} registros obtenidos.", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la extracción de Base de Datos.");
            }

            return results;
        }
    }
}
