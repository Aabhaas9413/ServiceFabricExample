using System;
using System.Fabric;
using Communicaton;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Product
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Product : StatefulService, IStateFullInterface
    {
        public Product(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<string> GetServiceDeails()
        {
            var serviceName = this.Context.ServiceName.ToString();
            var partitionId = Context.PartitionId.ToString();
            return $"{serviceName} :: {partitionId}";
        }

        public async Task<ProductDesc> GetFromQueue()
        {
            var stateManager = this.StateManager;
            var productQueue = await stateManager.GetOrAddAsync<IReliableQueue<ProductDesc>>("productqueue");
            using (var tx = stateManager.CreateTransaction())
            {
                var product = await productQueue.TryDequeueAsync(tx);
                await tx.CommitAsync();
                return product.Value;
            }
            throw new Exception();
        }

        public async Task AddToQueue(ProductDesc product)
        {
            var stateManager = this.StateManager;
            var productQueue = await stateManager.GetOrAddAsync<IReliableQueue<ProductDesc>>("productqueue");
            using (var tx = stateManager.CreateTransaction())
            {
                await productQueue.EnqueueAsync(tx, product);
                await tx.CommitAsync();
            }            
        }

        public async Task<ProductDesc> GetProductById(int id)
        {
            var stateManager = this.StateManager;
            var productDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, ProductDesc>>("productdict");
            using (var tx = stateManager.CreateTransaction())
            {
                var product = await productDict.TryGetValueAsync(tx, id);
                return product.Value;
            }
            throw new Exception();
        }
        public async Task AddProduct(ProductDesc product)
        {
            var stateManager = this.StateManager;
            var productDict = await stateManager.GetOrAddAsync<IReliableDictionary<int,ProductDesc>>("productdict");
            using(var tx = stateManager.CreateTransaction())
            {
                await productDict.AddOrUpdateAsync(tx, product.ID, product, (k,v)=>v);
                await tx.CommitAsync();
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
