using System.Threading.Tasks;

namespace EtlProject.Worker.Interfaces
{
    public interface IDimensionLoader
    {
        Task LoadDimensionsAsync();
    }
}
