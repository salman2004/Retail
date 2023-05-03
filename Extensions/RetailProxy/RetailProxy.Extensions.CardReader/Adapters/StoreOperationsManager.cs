/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace CDC
{
    namespace Commerce.RetailProxy.CardReader.Adapters
    {
        using System.Threading.Tasks;
        using CDC.RetailServer.CardReader;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.RetailProxy.Adapters;
        using CDC.Commerce.Runtime.CardReader;

        /// <summary>
        /// Encapsulates extension functionality related to store operations management.
        /// </summary>
        internal class StoreOperationsManager : IStoreOperationsManager
        {
            
            public async Task<bool> AuthenticateCard(string cnicNumber, string cardNumber)
            {
                var response = await CommerceRuntimeManager.Runtime.ExecuteAsync<CardReaderResponse>(new CardReaderRequest(cardNumber, cnicNumber), null).ConfigureAwait(false);
                return response.IsCardActivated;
            }
        }
    }
}
