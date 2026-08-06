using EtlProject.Data.Contexts;
using EtlProject.Data.Entities.Dwh.Dimensions;
using EtlProject.Worker.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtlProject.Worker.Services
{
    public class DimensionLoaderService : IDimensionLoader
    {
        private readonly EtlDbContext _dbContext;
        private readonly ILogger<DimensionLoaderService> _logger;

        public DimensionLoaderService(EtlDbContext dbContext, ILogger<DimensionLoaderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task LoadDimensionsAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando Carga de Dimensiones desde el área de Staging...");

                await LoadDimTiempoAsync();
                await LoadDimFuentesAsync();
                await LoadDimClasificacionAsync();
                await LoadDimClientesAsync();
                await LoadDimProductosAsync();

                _logger.LogInformation("Carga de Dimensiones finalizada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la carga de dimensiones.");
            }
        }

        private async Task LoadDimTiempoAsync()
        {
            var fechasStaging = await _dbContext.ReviewStaging
                .Where(r => r.ReviewDate.HasValue)
                .Select(r => r.ReviewDate.Value.Date)
                .Distinct()
                .ToListAsync();

            var fechasExistentes = await _dbContext.Dim_Tiempo
                .Select(d => d.Fecha)
                .ToListAsync();

            var fechasFaltantes = fechasStaging.Except(fechasExistentes).ToList();

            if (fechasFaltantes.Any())
            {
                var nuevasDimensiones = fechasFaltantes.Select(f => new Dim_Tiempo
                {
                    Fecha = f,
                    Anio = f.Year,
                    Mes = f.Month,
                    Dia = f.Day,
                    Trimestre = (f.Month - 1) / 3 + 1,
                    DiaSemana = (int)f.DayOfWeek
                });

                await _dbContext.Dim_Tiempo.AddRangeAsync(nuevasDimensiones);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Insertados {Count} nuevos registros en Dim_Tiempo.", fechasFaltantes.Count);
            }
        }

        private async Task LoadDimFuentesAsync()
        {
            var fuentesStaging = await _dbContext.ReviewStaging
                .Where(r => !string.IsNullOrEmpty(r.SourceType))
                .Select(r => r.SourceType)
                .Distinct()
                .ToListAsync();

            var fuentesExistentes = await _dbContext.Dim_Fuentes
                .Select(d => d.NombreFuente)
                .ToListAsync();

            var fuentesFaltantes = fuentesStaging.Except(fuentesExistentes).ToList();

            if (fuentesFaltantes.Any())
            {
                var nuevasDimensiones = fuentesFaltantes.Select(f => new Dim_Fuentes
                {
                    NombreFuente = f
                });

                await _dbContext.Dim_Fuentes.AddRangeAsync(nuevasDimensiones);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Insertados {Count} nuevos registros en Dim_Fuentes.", fuentesFaltantes.Count);
            }
        }

        private async Task LoadDimClasificacionAsync()
        {
            if (!await _dbContext.Dim_Clasificacion.AnyAsync())
            {
                var clasificaciones = new[]
                {
                    new Dim_Clasificacion { Categoria = "Negativa", RangoInicio = 1, RangoFin = 2.99M },
                    new Dim_Clasificacion { Categoria = "Neutra", RangoInicio = 3, RangoFin = 3.99M },
                    new Dim_Clasificacion { Categoria = "Positiva", RangoInicio = 4, RangoFin = 5 }
                };

                await _dbContext.Dim_Clasificacion.AddRangeAsync(clasificaciones);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Insertados 3 registros base en Dim_Clasificacion.");
            }
        }

        private async Task LoadDimClientesAsync()
        {
            var clientesStaging = await _dbContext.ReviewStaging
                .Where(r => !string.IsNullOrEmpty(r.ClientId))
                .Select(r => r.ClientId)
                .Distinct()
                .ToListAsync();

            var clientesExistentes = await _dbContext.Dim_Clientes
                .Select(d => d.IdOriginal)
                .ToListAsync();

            var clientesFaltantes = clientesStaging.Except(clientesExistentes).ToList();

            if (clientesFaltantes.Any())
            {
                var nuevasDimensiones = clientesFaltantes.Select(c => new Dim_Clientes
                {
                    IdOriginal = c,
                    Nombre = $"Cliente {c}",
                    Correo = "desconocido@ejemplo.com"
                });

                await _dbContext.Dim_Clientes.AddRangeAsync(nuevasDimensiones);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Insertados {Count} nuevos registros en Dim_Clientes.", clientesFaltantes.Count);
            }
        }

        private async Task LoadDimProductosAsync()
        {
            var productosStaging = await _dbContext.ReviewStaging
                .Where(r => !string.IsNullOrEmpty(r.ProductId))
                .Select(r => r.ProductId)
                .Distinct()
                .ToListAsync();

            var productosExistentes = await _dbContext.Dim_Productos
                .Select(d => d.IdOriginal)
                .ToListAsync();

            var productosFaltantes = productosStaging.Except(productosExistentes).ToList();

            if (productosFaltantes.Any())
            {
                var nuevasDimensiones = productosFaltantes.Select(p => new Dim_Productos
                {
                    IdOriginal = p,
                    Nombre = $"Producto {p}",
                    Categoria = "Sin Categoría",
                    Precio = 0
                });

                await _dbContext.Dim_Productos.AddRangeAsync(nuevasDimensiones);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Insertados {Count} nuevos registros en Dim_Productos.", productosFaltantes.Count);
            }
        }
    }
}
