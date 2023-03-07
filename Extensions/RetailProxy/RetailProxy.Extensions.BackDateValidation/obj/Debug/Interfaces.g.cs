// <auto-generated />
namespace CDC.Commerce.RetailProxy.BackDateValidation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.RetailProxy;

    
    /// <summary>
    /// Interface for Store Operations Manager.
    /// </summary>
    [GeneratedCodeAttribute("CDC.Commerce.RetailProxy.BackDateValidation", "1.0")]
    public interface IStoreOperationsManager : IEntityManager
    {
        
        /// <summary>
        /// ValidateTime method.
        /// </summary>
        /// <param name="deviceDateTime">The deviceDateTime.</param>
        /// <returns>bool object.</returns>
        Task<bool> ValidateTime(string deviceDateTime);
    
    }
    
 }
