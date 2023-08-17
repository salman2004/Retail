
namespace CDC.Commerce.Runtime.FBRIntegration
{
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CalculateTenderDiscountRequestHandlerEx : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes => new[]
                {
                    typeof(CalculateTenderDiscountRequest)
                };

        public async Task<Response> Execute(Request request)
        {
            CalculateTenderDiscountRequest calculateTenderDiscountRequest = (CalculateTenderDiscountRequest)request;
            CalculateTenderDiscountResponse response = await this.ExecuteNextAsync<CalculateTenderDiscountResponse>(request).ConfigureAwait(false);

            if (response.TenderDiscountLine.DiscountAmount == decimal.Zero)
            {
                return response;
            }

            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);
            GetTenderDiscountValue(request.RequestContext, out decimal minDiscount);
            GetAskariTenderId(request.RequestContext, out string tenderId);
            GetMaxTenderDiscount(request.RequestContext, out decimal maxDiscountAmount);
            decimal discountPercentage = GetDiscountPercentage(request.RequestContext, tenderId, out string offerId);
            decimal totalCharges = await GetTotalChargesInCart(request.RequestContext, calculateTenderDiscountRequest.CartId);

            if (offerIds.Any(offer => offer == offerId)
                && calculateTenderDiscountRequest.TenderLine.Amount > maxDiscountAmount
                && discountPercentage != decimal.Zero)
            {
                response.TenderDiscountLine.DiscountAmount = minDiscount;
                response.TenderDiscountLine.PaymentAmount = calculateTenderDiscountRequest.TenderLine.Amount - minDiscount;
            }

            if (offerIds.Any(offer => offer == offerId)
                && calculateTenderDiscountRequest.TenderLine.Amount < maxDiscountAmount
                && discountPercentage != decimal.Zero)
            {
                response.TenderDiscountLine.DiscountAmount = decimal.Round(((calculateTenderDiscountRequest.TenderLine.Amount - totalCharges) * discountPercentage / 100), 5, MidpointRounding.AwayFromZero);
                response.TenderDiscountLine.PaymentAmount = decimal.Round((calculateTenderDiscountRequest.TenderLine.Amount - response.TenderDiscountLine.DiscountAmount), 5, MidpointRounding.AwayFromZero);
            }

            return response;
        }
        
        private void GetTenderDiscountOfferIds(RequestContext context, out List<string> offerIds)
        {
            offerIds = new List<string>();

            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountOfferIds = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTHEADER").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountOfferIds))
            {
                offerIds = tenderDiscountOfferIds.Split(';').ToList();
            }
        }

        private void GetTenderDiscountValue(RequestContext context, out decimal minDiscount)
        {
            minDiscount = decimal.Zero;

            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTVALUE").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountValue) && decimal.TryParse(tenderDiscountValue, out decimal value))
            {
                minDiscount = value;
            }
        }

        private void GetMaxTenderDiscount(RequestContext context, out decimal maxDiscount)
        {
            maxDiscount = decimal.Zero;

            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MAXDISCOUNTAMOUNT").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountValue) && decimal.TryParse(tenderDiscountValue, out decimal value))
            {
                maxDiscount = value;
            }
        }

        private void GetAskariTenderId(RequestContext context, out string tenderId)
        {
            tenderId = string.Empty;

            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountOfferIds = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("AskariCardTenderMethod").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountOfferIds))
            {
                tenderId = tenderDiscountOfferIds;
            }
        }

        private void GetUnitNotAllowedForFractionalSale(RequestContext context, out string tenderId)
        {
            tenderId = string.Empty;

            // Get the configuration parameters
            GetConfigurationParametersDataRequest configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            EntityDataServiceResponse<RetailConfigurationParameter> configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountOfferIds = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("UnitNotAllowedForFractionalSale").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountOfferIds))
            {
                tenderId = tenderDiscountOfferIds;
            }
        }

        public decimal GetDiscountPercentage(RequestContext context, string tenderTypeId, out string offerId)
        {
            ExtensionsEntity extensionsEntity;
            offerId = string.Empty;

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery
                {
                    QueryString = $@"Select R2.OFFERID, DISCOUNTVALUE from ax.RETAILTENDERDISCOUNT R1  JOIN ax.RETAILTENDERDISCOUNTTHRESHOLDTIERS R2 on R2.RECID = R1.RECID  JOIN ax.RETAILTENDERTYPETABLE R3 on R3.RECID = R1.RETAILTENDERTYPE WHERE R3.TENDERTYPEID = @tenderTypeId"
                };
                query.Parameters["@tenderTypeId"] = tenderTypeId;

                try
                {
                    extensionsEntity = databaseContext.ReadEntity<ExtensionsEntity>(query).FirstOrDefault();
                    offerId = extensionsEntity.GetProperty("OFFERID").ToString();
                    return Convert.ToDecimal(extensionsEntity?.GetProperty("DISCOUNTVALUE")?.ToString() ?? decimal.Zero.ToString());

                }
                catch (Exception)
                {
                    offerId = string.Empty;
                    return decimal.Zero;
                }
            }
        }

        public async Task<decimal> GetTotalChargesInCart(RequestContext context, string cartId)
        {
            if (!string.IsNullOrEmpty(cartId))
            {
                GetCartRequest getCartRequest = new GetCartRequest(new CartSearchCriteria(cartId), QueryResultSettings.SingleRecord) { RequestContext = context };
                GetCartResponse getCartResponse = await context.ExecuteAsync<GetCartResponse>(getCartRequest).ConfigureAwait(false);
                return getCartResponse.Transactions.FirstOrDefault().TotalHeaderSalesCharges();
            }
            return decimal.Zero;
        }
    }
}
