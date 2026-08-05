using EtlProject.Data.Entities.Staging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtlProject.Worker.Interfaces
{
    public interface IDataLoader
    {
        Task LoadToStagingAsync(IEnumerable<ReviewStaging> data);
    }
}
