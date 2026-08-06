using System.Threading.Tasks;

namespace EtlProject.Worker.Interfaces
{
    public interface IFactLoader
    {
        Task LoadFactsAsync();
    }
}
