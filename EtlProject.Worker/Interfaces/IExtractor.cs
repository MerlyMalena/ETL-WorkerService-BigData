using EtlProject.Data.Entities.Staging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtlProject.Worker.Interfaces
{
    public interface IExtractor
    {
        Task<IEnumerable<ReviewStaging>> ExtractAsync();
    }
}
