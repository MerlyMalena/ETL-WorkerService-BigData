using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtlProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialCommentsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetComments()
        {
            // Ruta relativa hacia el archivo CSV que está en el proyecto Worker
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "EtlProject.Worker", "social_comments.csv");
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"No se encontró el archivo CSV en: {filePath}" });
            }

            var comments = new List<object>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath);
            
            // Regex para separar por comas ignorando las comas que están dentro de comillas
            var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            // Saltamos la primera línea (encabezados) y recorremos el resto
            foreach (var line in lines.Skip(1))
            {
                var parts = csvParser.Split(line);
                
                if (parts.Length >= 6)
                {
                    comments.Add(new 
                    {
                        Id = parts[0].Trim('"'),
                        User = parts[1].Trim('"'),
                        Product = parts[2].Trim('"'),
                        Date = parts[4].Trim('"'),
                        Text = parts[5].Trim('"')
                       
                    });
                }
            }

            return Ok(comments);
        }
    }
}
