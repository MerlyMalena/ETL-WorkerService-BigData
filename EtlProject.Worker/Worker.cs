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
            // Se ejecuta una vez por ciclo, o podrías usar un temporizador (ej. cada 24 hrs)
            // Para propósitos académicos, simularemos que corre de inmediato.
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

                    // 1. EXTRAER (E)
                    // Rendimiento: Uso de paralelismo (async/await) para llamar a todas las fuentes simultáneamente.
                    var extractionTasks = extractors.Select(e => e.ExtractAsync());
                    
                    _logger.LogInformation("Lanzando tareas de extracción en paralelo...");
                    var resultsArray = await Task.WhenAll(extractionTasks);

                    // Consolidar resultados
                    var allExtractedRecords = new List<ReviewStaging>();
                    foreach (var resultList in resultsArray)
                    {
                        allExtractedRecords.AddRange(resultList);
                    }
                    
                    _logger.LogInformation("Extracción completada. Total registros consolidados: {Count}", allExtractedRecords.Count);

                    // 2. CARGAR A STAGING
                    if (allExtractedRecords.Any())
                    {
                        await dataLoader.LoadToStagingAsync(allExtractedRecords);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "El proceso ETL ha fallado de manera crítica.");
                }
                finally
                {
                    stopwatch.Stop();
                    _logger.LogInformation("Proceso ETL finalizado en {ElapsedMilliseconds} ms.", stopwatch.ElapsedMilliseconds);
                }

                // Simulamos esperar 24 horas para la siguiente ejecución, 
                // pero para pruebas lo dejamos en un loop largo o rompemos.
                _logger.LogInformation("Esperando para el próximo ciclo ETL...");
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
