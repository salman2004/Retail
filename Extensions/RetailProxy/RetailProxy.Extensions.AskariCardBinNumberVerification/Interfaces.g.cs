// <auto-generated />
namespace CDC.Commerce.RetailProxy.AskariCardBinNumberVerification
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
    [GeneratedCodeAttribute("CDC.Commerce.RetailProxy.AskariCardBinNumberVerification", "1.0")]
    public interface IStoreOperationsManager : IEntityManager
    {
        
        /// <summary>
        /// ValidateBinNumber method.
        /// </summary>
        /// <param name="cardNumber">The cardNumber.</param>
        /// <param name="transactionId">The transactionId.</param>
        /// <returns>bool object.</returns>
        Task<bool> ValidateBinNumber(string cardNumber, string transactionId);
    
    }
    
 }