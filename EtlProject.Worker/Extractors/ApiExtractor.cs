using EtlProject.Data.Entities.Staging;
using EtlProject.Worker.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EtlProject.Worker.Extractors
{
    public class ApiExtractor : IExtractor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiExtractor> _logger;

        public ApiExtractor(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiExtractor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewStaging>> ExtractAsync()
        {
            var results = new List<ReviewStaging>();
            var apiUrl = _configuration["ApiSettings:SocialCommentsUrl"];

            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("URL de API REST 'ApiSettings:SocialCommentsUrl' no configurada.");
                return results;
            }

            try
            {
                _logger.LogInformation("Iniciando extracción desde API REST: {ApiUrl}", apiUrl);
                
                var client = _httpClientFactory.CreateClient("SocialCommentsClient");
                
                // Realizamos el GET al API
                var response = await client.GetFromJsonAsync<List<ApiCommentDto>>(apiUrl);

                if (response != null)
                {
                    foreach (var item in response)
                    {
                        results.Add(new ReviewStaging
                        {
                            SourceType = "API",
                            ReviewId = item.Id,
                            ClientId = item.User,
                            ProductId = item.Product,
                            Comment = item.Text,
                            Rating = item.Score,
                            ReviewDate = TryParseDate(item.Date),
                            ExtractionDate = DateTime.UtcNow
                        });
                    }
                }
                
                _logger.LogInformation("Extracción API completada. {Count} registros obtenidos.", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la extracción de la API REST.");
            }

            return results;
        }

        private DateTime? TryParseDate(string? dateStr)
        {
            if (dateStr != null && DateTime.TryParse(dateStr, out DateTime date))
                return date;
            return null;
        }

        // DTO Interno para deserializar la respuesta del API mockeado
        private class ApiCommentDto
        {
            public string? Id { get; set; }
            public string? User { get; set; }
            public string? Product { get; set; }
            public string? Text { get; set; }
            public decimal? Score { get; set; }
            public string? Date { get; set; }
        }
    }
}
