using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CDC.Commerce.Runtime.FractionalSale;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using System.Linq;

namespace CDC.RetailServer.FractionalSale
{
    public class FractionalSaleController : IController
    {    
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public async Task<bool> ValidateFractionalSale(IEndpointContext context, IEnumerable<ProductInformation> ProductsInformation)
        {
            try
            {
                var products = ProductsInformation.ToList();
                var request = new FractionSaleRequest() { ProductsInformation = products };
                var response = await context.ExecuteAsync<FractionSaleResponse>(request).ConfigureAwait(false);
                return response.Status;
            }
            catch (Exception exception)
            {
                throw new CommerceException("Retail Server", exception.Message);
            }
        }
    }
}
