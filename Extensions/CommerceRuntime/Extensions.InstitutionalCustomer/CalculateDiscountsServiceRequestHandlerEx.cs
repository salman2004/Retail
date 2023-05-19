namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
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
            if (!request.Transaction.LoyaltyCardId.IsNullOrEmpty() && request.Transaction.IsPropertyDefined("CSDCardBalance") && !char.IsDigit(request.Transaction.LoyaltyCardId[0]) && Convert.ToBoolean(request.Transaction?.GetProperty("checkLoyaltyLimit")?.ToString() ?? "false"))
            {
                var affiliation = request.Transaction.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).FirstOrDefault();
                GetAffiliationDiscounts(request.RequestContext, affiliation?.AffiliationId.ToString() ?? string.Empty , out List<ExtensionsEntity> discountExtensionEntity);
                decimal.TryParse(request.Transaction?.GetProperty("CSDCardBalance")?.ToString()?.Trim() ?? string.Empty, out decimal cardBalance);
                int balance = (int)request.Transaction.ActiveSalesLines.Where(sl => sl.DiscountAmount > 0 && !sl.DiscountLines.IsNullOrEmpty() && sl.DiscountLines.Any(dl=> discountExtensionEntity.Any(de => de.GetProperty("OFFERID").ToString() == dl.OfferId))).Sum(sl => sl.Price * sl.QuantityDiscounted);
                request.Transaction.SetProperty("CSDMonthlyLimitUsed", Convert.ToString(cardBalance - balance).PadLeft(5, '0'));
            }
            
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

        private static void GetAffiliationDiscounts(RequestContext context, string affiliationId, out List<ExtensionsEntity> entities)
        {
            if (affiliationId == null || affiliationId.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select R2.OFFERID from ax.RETAILAFFILIATIONPRICEGROUP R1 JOIN ax.RetailDiscountPriceGroup R2 on R2.PRICEDISCGROUP = R1.PRICEDISCGROUP WHERE R1.RETAILAFFILIATION = @affiliationId AND R2.DATAAREAID = @dataAreaId";
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@affiliationId"] = affiliationId;

                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                }
                catch (Exception)
                {
                    entities = new List<ExtensionsEntity>();
                }
            }
        }
    }
}
