namespace CDC.RetailServer.BackDateValidation
{
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
    using System.Threading.Tasks;
    using System;
    using CDC.Commerce.Runtime.AskariCardBinNumberVerification;
    using CDC.Commerce.Runtime.AskariCardBinNumberVerification.Model;

    /// <summary>
    /// The controller to Check if MPOS DateTine is correct.
    /// </summary>
    /// 
    public class AskariCardBinNumberVerificationController : IController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Application, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee, CommerceRoles.Storefront)]
        public virtual async Task<bool> ValidateBinNumber(IEndpointContext context, string cardNumber, string transactionId)
        {
            var request = new AskariCardBinVerificationRequest(cardNumber, transactionId);
            var response = await context.ExecuteAsync<AskariCardBinVerificationResponse>(request).ConfigureAwait(false);
            return response.IsDateValidated;
        }
    }
}
