using Communicaton;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommunicationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommunicationController : ControllerBase
    {
        [HttpGet]
        [Route("stateless")]
        public async Task<string> StateLessGet()
        {
            var sp = ServiceProxy.Create<IStateLessInterface>(new Uri("fabric:/ServiceFabricExample/ConsumerAnalytics"));
            var serviceName = await sp.GetServiceDeails();
            return serviceName;
        }

        [HttpGet]
        [Route("statefull")]
        public async Task<string> StateFullGet([FromQuery] int productId)
        {
            var partitionId = productId % 3;
            var sp = ServiceProxy.Create<IStateFullInterface>(new Uri("fabric:/ServiceFabricExample/Product"),
                new ServicePartitionKey(partitionId));
            var serviceName = await sp.GetServiceDeails();
            return serviceName;
        }

        [HttpPost]
        [Route("addproduct")]
        public async Task AddProduct([FromBody] ProductDesc product)
        {
            var partitionId = product.ID % 3;
            var sp = ServiceProxy.Create<IStateFullInterface>(new Uri("fabric:/ServiceFabricExample/Product"),
                new ServicePartitionKey(partitionId));
            await sp.AddProduct(product);
           
        }

        [HttpPost]
        [Route("addqueue")]
        public async Task AddToQueue([FromQuery] int partitionID , [FromBody] ProductDesc product)
        {            
            var sp = ServiceProxy.Create<IStateFullInterface>(new Uri("fabric:/ServiceFabricExample/Product"),
                new ServicePartitionKey(partitionID));
            await sp.AddToQueue(product);

        }

        [HttpGet]
        [Route("getproduct")]
        public async Task<ProductDesc> GetProduct([FromQuery] int productId)
        {
            var partitionId = productId % 3;
            var sp = ServiceProxy.Create<IStateFullInterface>(new Uri("fabric:/ServiceFabricExample/Product"),
                new ServicePartitionKey(partitionId));
            var product = await sp.GetProductById(productId);
            return product;
        }

        [HttpGet]
        [Route("getqueue")]
        public async Task<ProductDesc> GetFromQueue([FromQuery] int partitionId)
        {
            var sp = ServiceProxy.Create<IStateFullInterface>(new Uri("fabric:/ServiceFabricExample/Product"),
                new ServicePartitionKey(partitionId));
            var product = await sp.GetFromQueue();
            return product;
        }
    }
}
