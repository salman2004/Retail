using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.FractionalSale
{
   public class FractionalSaleTrigger : IRequestTriggerAsync
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
                    typeof(UpdateCartLinesRequest)
                };
            }
        }

        public async Task OnExecuted(Request request, Response response)
        {
            await Task.CompletedTask;
        }

        public async Task OnExecuting(Request request)
        {
            if (request is UpdateCartLinesRequest)
            {
                try
                {
                    ChannelConfiguration channelConfigs = request.RequestContext.GetChannelConfiguration();
                    var updateCartLine = (UpdateCartLinesRequest)request;
                    var fractionSaleRequest = new FractionSaleRequest();
                    fractionSaleRequest.ProductsInformation = new List<ProductInformation>();
                    foreach (var cartLine in updateCartLine.CartLines)
                    {
                        if (!(cartLine.Quantity % 1 == 0))
                        {
                            fractionSaleRequest.ProductsInformation = new List<ProductInformation>();

                            fractionSaleRequest.ProductsInformation.Add(new ProductInformation()
                            {
                                ProductId = cartLine.ProductId,
                                RetailStoreId = channelConfigs.InventLocation,
                                UnitOfMeasure = cartLine.UnitOfMeasureSymbol
                            }
                            );
                            var response = await request.RequestContext.ExecuteAsync<FractionSaleResponse>(fractionSaleRequest).ConfigureAwait(false);

                            if (!response.Status)
                            {
                                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "The product is not authorized for fractional sale.");
                            }
                        }
                    }

                }
                catch (Exception exception)
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Custom error")
                    {
                        LocalizedMessage = exception.Message

                };
            }

            }

            await Task.CompletedTask;
        }
    }
}
