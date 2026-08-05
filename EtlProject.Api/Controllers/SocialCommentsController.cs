using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
            
            await Task.Delay(500);

            var comments = new List<object>
            {
                new { Id = "API_001", User = "C001", Product = "P001", Text = "Excelente producto, muy recomendado", Score = 5, Date = "2023-10-01T10:00:00Z" },
                new { Id = "API_002", User = "C002", Product = "P002", Text = "Llegó roto, pésimo servicio", Score = 1, Date = "2023-10-02T11:30:00Z" },
                new { Id = "API_003", User = "C003", Product = "P001", Text = "Cumple su función, pero podría ser mejor", Score = 3, Date = "2023-10-03T15:45:00Z" }
            };

            return Ok(comments);
        }
    }
}
