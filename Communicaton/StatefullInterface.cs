using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace Communicaton
{
    public interface IStateFullInterface: IService
    {
         Task<string> GetServiceDeails();
         Task<ProductDesc> GetProductById(int id);
         Task AddProduct(ProductDesc product);
        Task<ProductDesc> GetFromQueue();
        Task AddToQueue(ProductDesc product);
    }
}
