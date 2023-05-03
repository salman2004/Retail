using System.Collections.Generic;
using System.Threading.Tasks;
using CDC.Commerce.Runtime.FractionalSale;
using Microsoft.Dynamics.Commerce.RetailProxy.Adapters;
using System.Linq;

namespace CDC.Commerce.RetailProxy.FractionalSale.Adapters
{
    public class StoreOperationsManager : IStoreOperationsManager
    {

        public async Task<bool> ValidateFractionalSale(IEnumerable<Runtime.FractionalSale.ProductInformation> productsInformation)
        {
            var products = productsInformation.ToList();
            var request = new FractionSaleRequest() { ProductsInformation = products };
            var response = await CommerceRuntimeManager.Runtime.ExecuteAsync<FractionSaleResponse>(request, null).ConfigureAwait(false);
            return response.Status;
        }
    }


}
