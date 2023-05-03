namespace CDC.RetailServer.BackDateValidation
{
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
    using System.Threading.Tasks;
    using System;
    using CDC.Commerce.Runtime.BackDateValidation;
    using CDC.Commerce.Runtime.BackDateValidation.Model;

    /// <summary>
    /// The controller to Check if MPOS DateTine is correct.
    /// </summary>
    /// 
    public class BackDateValidationController : IController
    {
        /// <summary>
        /// Performs DateTime Validation.
        /// </summary>
        /// <param name="parameters">The dictionary of action parameter values.</param>
        /// <returns>Boolean value wether the datetime was correct</returns>
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Application, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee, CommerceRoles.Storefront)]
        public virtual async Task<bool> ValidateTime(IEndpointContext context, string deviceDateTime)
        {
            var request = new BackDateValidationRequest(deviceDateTime);
            var response = await context.ExecuteAsync<BackDateValidationResponse>(request).ConfigureAwait(false);
            return response.IsDateValidated;
        }
    }
}
