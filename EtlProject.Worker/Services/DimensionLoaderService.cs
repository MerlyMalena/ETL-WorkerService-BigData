using EtlProject.Data.Contexts;
using EtlProject.Data.Entities.Dwh.Dimensions;
using EtlProject.Worker.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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
                var culturaEspanol = new System.Globalization.CultureInfo("es-ES");
                var nuevasDimensiones = fechasFaltantes.Select(f => new Dim_Tiempo
                {
                    IdFecha = int.Parse(f.ToString("yyyyMMdd")),
                    Fecha = f,
                    Anio = f.Year,
                    Mes = f.Month,
                    NombreMes = culturaEspanol.DateTimeFormat.GetMonthName(f.Month),
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
                .Select(d => d.TipoFuente)
                .ToListAsync();

            var fuentesFaltantes = fuentesStaging.Except(fuentesExistentes).ToList();

            if (fuentesFaltantes.Any())
            {
                var nuevasDimensiones = fuentesFaltantes.Select(f => new Dim_Fuentes
                {
                    IdFuente = f == "CSV" ? 1 : f == "API" ? 2 : f == "DB" ? 3 : 99,
                    TipoFuente = f,
                    NombreFuente = $"Fuente {f}"
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
                    new Dim_Clasificacion { NombreClasificacion = "Negativa" },
                    new Dim_Clasificacion { NombreClasificacion = "Neutra" },
                    new Dim_Clasificacion { NombreClasificacion = "Positiva" }
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

            var clientesStagingInt = clientesStaging.Select(c => 
            {
                string idNumberStr = new string(c.Where(char.IsDigit).ToArray());
                int.TryParse(idNumberStr, out int id);
                return id;
            }).Distinct().ToList();

            var clientesExistentes = await _dbContext.Dim_Clientes
                .Select(d => d.IdCliente)
                .ToListAsync();

            var clientesFaltantes = clientesStagingInt.Except(clientesExistentes).ToList();

            if (clientesFaltantes.Any())
            {
                var nuevasDimensiones = clientesFaltantes.Select(id => new Dim_Clientes
                {
                    IdCliente = id,
                    Nombre = $"Cliente C{id:D3}",
                    Email = "desconocido@ejemplo.com"
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

            var productosStagingInt = productosStaging.Select(p => 
            {
                string idNumberStr = new string(p.Where(char.IsDigit).ToArray());
                int.TryParse(idNumberStr, out int id);
                return id;
            }).Distinct().ToList();

            var productosExistentes = await _dbContext.Dim_Productos
                .Select(d => d.IdProducto)
                .ToListAsync();

            var productosFaltantes = productosStagingInt.Except(productosExistentes).ToList();

            if (productosFaltantes.Any())
            {
                var nuevasDimensiones = productosFaltantes.Select(id => new Dim_Productos
                {
                    IdProducto = id,
                    Nombre = $"Producto P{id:D3}",
                    Categoria = "Sin Categoría"
                });

                await _dbContext.Dim_Productos.AddRangeAsync(nuevasDimensiones);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Insertados {Count} nuevos registros en Dim_Productos.", productosFaltantes.Count);
            }
        }
    }
}
