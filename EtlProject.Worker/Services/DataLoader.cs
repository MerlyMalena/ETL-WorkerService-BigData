using EtlProject.Data.Contexts;
using EtlProject.Data.Entities.Staging;
using EtlProject.Data.Entities.Dwh.Dimensions;
using EtlProject.Data.Entities.Dwh.Facts;
using EtlProject.Worker.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtlProject.Worker.Services
{
    public class DataLoader : IDataLoader
    {
        private readonly EtlDbContext _dbContext;
        private readonly ILogger<DataLoader> _logger;

        public DataLoader(EtlDbContext dbContext, ILogger<DataLoader> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task LoadToStagingAsync(IEnumerable<ReviewStaging> data)
        {
            try
            {
                _logger.LogInformation("Iniciando carga de datos consolidados en la capa de Staging...");
                
              
                _logger.LogInformation("Verificando existencia de la Base de Datos Staging...");
                
                await _dbContext.Database.EnsureCreatedAsync(); 

                await _dbContext.Database.ExecuteSqlRawAsync(
                    "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'staging') BEGIN EXEC('CREATE SCHEMA [staging]') END;"
                );

                var databaseCreator = _dbContext.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IDatabaseCreator>() as Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    try { await databaseCreator.CreateTablesAsync(); } catch { }
                }

                // Staging (Insertar los datos)
                await _dbContext.ReviewStaging.AddRangeAsync(data);
                var savedCount = await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Extracción y consolidación finalizada exitosamente. {Count} registros insertados en la tabla de Staging listos para la futura fase de Transformación y Carga.", savedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los datos en la base de datos de Staging.");
            }
        }
    }
}
