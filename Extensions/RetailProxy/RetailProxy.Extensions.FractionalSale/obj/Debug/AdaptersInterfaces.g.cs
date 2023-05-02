// <auto-generated />
namespace CDC.Commerce.RetailProxy.FractionalSale.Adapters
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;

    
    /// <summary>
    /// Interface for Store Operations Manager.
    /// </summary>
    [GeneratedCodeAttribute("CDC.Commerce.RetailProxy.FractionalSale", "1.0")]
    public interface IStoreOperationsManager : Microsoft.Dynamics.Commerce.RetailProxy.IEntityManager
    {
        
        /// <summary>
        /// ValidateFractionalSale method.
        /// </summary>
        /// <param name="productsInformation">The productsInformation.</param>
        /// <returns>bool object.</returns>
        Task<bool> ValidateFractionalSale(IEnumerable<CDC.Commerce.Runtime.FractionalSale.ProductInformation> productsInformation);
    
    }
    
 }
