namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using PE = Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine;

    public class CalculateDiscountsServiceRequestHandlerEx : IRequestHandlerAsync
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
                    typeof(CalculateDiscountsServiceRequest)
                };
            }
        }

        /// <summary>
        /// Implements customized solutions for pricing services.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The response object.</returns>
        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            using (new PE.PricingEngineExtensionContext())
            {
                Type requestType = request.GetType();
                using (var profiler = new PE.SimpleProfiler(requestType.Name, true, 0))
                {
                    Response response;
                    if (requestType == typeof(CalculateDiscountsServiceRequest))
                    {
                        response = await CalculateDiscountAsync((CalculateDiscountsServiceRequest)request).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                    }

                    return response;
                }
            }
        }

        private static async Task<GetPriceServiceResponse> CalculateDiscountAsync(CalculateDiscountsServiceRequest request)
        {
            ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
            Customer customer = await GetCustomerAsync(request.RequestContext, request.Transaction.CustomerId).ConfigureAwait(false);
            PricingDataServiceManagerEx pricingDataServiceManagerEx = new PricingDataServiceManagerEx(request.RequestContext, request.Transaction);
            
            PE.PricingEngine.CalculateDiscountsForLines(
                pricingDataServiceManagerEx,
                request.Transaction,
                new ChannelCurrencyOperations(request.RequestContext),
                channelConfiguration.Currency,
                customer.LineDiscountGroup,
                customer.MultilineDiscountGroup,
                customer.TotalDiscountGroup,
                shouldTotalLines: true,
                request.CalculateSimpleDiscountOnly,
                request.RequestContext.GetNowInChannelTimeZone(),
                new PricingTransactionTotalingEx(request.RequestContext));

            request.Transaction.IsDiscountFullyCalculated = !request.CalculateSimpleDiscountOnly;

            request.Transaction.SetProperty("CSDstoreId", request.RequestContext.GetDeviceConfiguration().StoreNumber);
            
            return new GetPriceServiceResponse(request.Transaction);
        }

        private static async Task<Customer> GetCustomerAsync(RequestContext context, string customerAccount)
        {
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(customerAccount))
            {
                var getCustomerDataRequest = new GetCustomerDataRequest(customerAccount);
                SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = await context.ExecuteAsync<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest).ConfigureAwait(false);
                customer = getCustomerDataResponse.Entity;
            }

            return customer ?? (new Customer());
        }
    }
}
