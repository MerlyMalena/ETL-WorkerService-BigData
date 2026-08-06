using EtlProject.Worker.Interfaces;
using EtlProject.Data.Entities.Staging;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EtlProject.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Se ejecuta una vez por ciclo
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Iniciando Proceso ETL (Fase de Extracción) a las: {time}", DateTimeOffset.Now);
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    // Crear un scope para resolver servicios Scoped (como DbContext) en un Singleton (BackgroundService)
                    using var scope = _serviceProvider.CreateScope();
                    
                    // Obtener dependencias
                    var extractors = scope.ServiceProvider.GetRequiredService<IEnumerable<IExtractor>>();
                    var dataLoader = scope.ServiceProvider.GetRequiredService<IDataLoader>();

                    var extractionTasks = extractors.Select(e => e.ExtractAsync());
                    
                    _logger.LogInformation("Lanzando tareas de extracción en paralelo...");
                    var resultsArray = await Task.WhenAll(extractionTasks);

                    
                    var allExtractedRecords = new List<ReviewStaging>();
                    foreach (var resultList in resultsArray)
                    {
                        allExtractedRecords.AddRange(resultList);
                    }
                    
                    _logger.LogInformation("Extracción completada. Total registros consolidados: {Count}", allExtractedRecords.Count);

                    // Staging
                    if (allExtractedRecords.Any())
                    {
                        await dataLoader.LoadToStagingAsync(allExtractedRecords);
                        
                        // Fase 2: Carga de Dimensiones
                        var dimensionLoader = scope.ServiceProvider.GetRequiredService<IDimensionLoader>();
                        await dimensionLoader.LoadDimensionsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "El proceso de extracción ha fallado de manera crítica.");
                }
                finally
                {
                    stopwatch.Stop();
                    _logger.LogInformation("Proceso de extracción finalizado en {ElapsedMilliseconds} ms.", stopwatch.ElapsedMilliseconds);
                }

                _logger.LogInformation("Esperando para el próximo ciclo ETL...");
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
