using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace Communicaton
{
    public interface IStateLessInterface: IService
    {
         Task<string> GetServiceDeails();
    }
}
