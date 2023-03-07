namespace CDC.Commerce.Runtime.ReturnTransactions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

    public class ReturnTransactionDataService : IRequestHandlerAsync
    {
        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(AddCartLinesRequest)
                    
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            try
            {
                string errorMessage = string.Empty;
                var channelConfigs = request.RequestContext.GetChannelConfiguration();
                AddCartLinesRequest returnTransaction = (AddCartLinesRequest)request;
                if (returnTransaction != null)
                {
                    foreach (var returnLine in returnTransaction.CartLines.Where(cl => cl.ReturnTransactionId != string.Empty))
                    {
                        InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("ValidateReturnTransaction", returnLine.ReturnTransactionId, channelConfigs.RetailFunctionalityProfileId, channelConfigs.InventLocationDataAreaId);
                        InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                        var results = response.Result;

                        if (!(bool)results[0])
                        {
                            errorMessage += Convert.ToString(results[1]);
                        }

                    }
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        throw new CommerceException("Retail Server", errorMessage);
                    }
                    else
                    {
                        return await this.ExecuteNextAsync<Response>(request);
                    }
                }
                else
                {
                    return await this.ExecuteNextAsync<Response>(request);
                }
            }
            catch (Exception exception)
            {
                throw new CommerceException("Retail Server", exception.Message);
            }
        }
    }
}
