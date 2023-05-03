using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
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

                    var UnitNotAllowedForFractionalSale = GetRetailConfigurationParameter(request, "UnitNotAllowedForFractionalSale", channelConfigs.InventLocationDataAreaId);
                    if (!string.IsNullOrWhiteSpace(UnitNotAllowedForFractionalSale))
                    {
                        var factionalSalesUnits = UnitNotAllowedForFractionalSale.ToLower().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        foreach (var cartLine in updateCartLine.CartLines)
                        {
                            if (!(cartLine.Quantity % 1 == 0))
                            {
                                if (factionalSalesUnits.Any(x => x == cartLine.UnitOfMeasureSymbol?.ToLower()))
                                {
                                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "The product is not authorized for fractional sale.");
                                }

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
        private string GetRetailConfigurationParameter(Request request, string name, string company)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string result = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (name).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
            return result;
        }
    }


}
