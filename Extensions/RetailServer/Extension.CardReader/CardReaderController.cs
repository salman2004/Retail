/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace CDC.RetailServer.CardReader
{
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
    using CDC.Commerce.Runtime.CardReader;

    /// <summary>
    /// The controller to retrieve sales transaction signature operations.
    /// </summary>
    /// 
    public class CardReaderController : IController
    {    
        /// <summary>
        /// Performs getting last terminal sequential signature operation.
        /// </summary>
        /// <param name="parameters">The dictionary of action parameter values.</param>
        /// <returns>The last terminal sequential signature data.</returns>
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Application, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee, CommerceRoles.Storefront)]
        public virtual async Task<bool> AuthenticateCard(IEndpointContext context, string cnicNumber, string cardNumber)
        {
            var request = new CardReaderRequest(cardNumber, cnicNumber);
            var response = await context.ExecuteAsync<CardReaderResponse>(request).ConfigureAwait(false);
            return response.IsCardActivated;
        }
    }
}
