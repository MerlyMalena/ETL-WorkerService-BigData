using EtlProject.Data.Contexts;
using EtlProject.Worker;
using EtlProject.Worker.Extractors;
using EtlProject.Worker.Interfaces;
using EtlProject.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace EtlProject.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/etl_log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Iniciando el Worker Service del ETL...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "El servicio falló al iniciar correctamente");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var analyticConnection = hostContext.Configuration.GetConnectionString("AnalyticConnection");
                    services.AddDbContext<EtlDbContext>(options =>
                        options.UseSqlServer(analyticConnection));

                    services.AddHttpClient("SocialCommentsClient");

                    services.AddTransient<IExtractor, CsvExtractor>();
                    services.AddScoped<IExtractor, DatabaseExtractor>();
                    services.AddScoped<IExtractor, ApiExtractor>();
                    services.AddScoped<IDataLoader, DataLoader>();
                    services.AddScoped<IDimensionLoader, DimensionLoaderService>(); // Servicio de Fase 2

                    services.AddHostedService<Worker>();
                });
    }
}
