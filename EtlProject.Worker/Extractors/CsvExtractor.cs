using CsvHelper;
using CsvHelper.Configuration;
using EtlProject.Data.Entities.Staging;
using EtlProject.Worker.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace EtlProject.Worker.Extractors
{
    public class CsvExtractor : IExtractor
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsvExtractor> _logger;

        public CsvExtractor(IConfiguration configuration, ILogger<CsvExtractor> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewStaging>> ExtractAsync()
        {
            var results = new List<ReviewStaging>();
            var filePath = _configuration["FileSettings:CsvPath"];

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _logger.LogWarning("Archivo CSV no encontrado en la ruta: {FilePath}", filePath);
                return results;
            }

            try
            {
                _logger.LogInformation("Iniciando extracción desde CSV: {FilePath}", filePath);
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);

                var records = csv.GetRecordsAsync<CsvSurveyRecord>();

                await foreach (var record in records)
                {
                    results.Add(new ReviewStaging
                    {
                        SourceType = "CSV",
                        ReviewId = record.SurveyId,
                        ClientId = record.UserId,
                        ProductId = record.ProductId,
                        Comment = record.Feedback,
                        Rating = record.Score,
                        ReviewDate = TryParseDate(record.Date),
                        ExtractionDate = DateTime.UtcNow
                    });
                }
                
                _logger.LogInformation("Extracción CSV completada. {Count} registros obtenidos.", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la extracción CSV.");
            }

            return results;
        }

        private DateTime? TryParseDate(string? dateStr)
        {
            if (dateStr != null && DateTime.TryParse(dateStr, out DateTime date))
                return date;
            return null;
        }

        // DTO Interno para mapear el CSV
        private class CsvSurveyRecord
        {
            [CsvHelper.Configuration.Attributes.Name("IdOpinion")]
            public string? SurveyId { get; set; }
            
            [CsvHelper.Configuration.Attributes.Name("IdCliente")]
            public string? UserId { get; set; }
            
            [CsvHelper.Configuration.Attributes.Name("IdProducto")]
            public string? ProductId { get; set; }
            
            [CsvHelper.Configuration.Attributes.Name("Comentario")]
            public string? Feedback { get; set; }
            
            [CsvHelper.Configuration.Attributes.Name("PuntajeSatisfacción")]
            public decimal? Score { get; set; }
            
            [CsvHelper.Configuration.Attributes.Name("Fecha")]
            public string? Date { get; set; }
        }
    }
}
