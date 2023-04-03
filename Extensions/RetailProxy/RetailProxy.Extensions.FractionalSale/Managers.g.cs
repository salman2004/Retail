// <auto-generated />
namespace CDC.Commerce.RetailProxy.FractionalSale
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.RetailProxy;
    
    /// <summary>
    /// Class implements Store Operations Manager.
    /// </summary>
    [GeneratedCodeAttribute("CDC.Commerce.RetailProxy.FractionalSale", "1.0")]
    internal class StoreOperationsManager : IStoreOperationsManager
    {
        private IContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreOperationsManager"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public StoreOperationsManager(IContext context)
        {
            this.context = context;
        }
    
        
        /// <summary>
        /// ValidateFractionalSale method.
        /// </summary>
        /// <param name="productsInformation">The productsInformation.</param>
        /// <returns>bool object.</returns>
        public async Task<bool> ValidateFractionalSale(IEnumerable<ProductInformation> productsInformation)
        {       
            return await this.context.ExecuteOperationSingleResultAsync<bool>(
                "",
                "StoreOperations",
                "ValidateFractionalSale",
                true, null, OperationParameter.Create("ProductsInformation", productsInformation, false));
        }
        
    }
    
 }