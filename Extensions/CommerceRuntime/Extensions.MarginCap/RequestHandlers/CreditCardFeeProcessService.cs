using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace CDC.Commerce.Runtime.MarginCap.RequestHandlers
{
    public class CreditCardFeeProcessService : IRequestHandlerAsync
    {
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

        public async Task<Response> Execute(Request request)
        {   
            GetPriceServiceResponse priceServiceResponse = new GetPriceServiceResponse();
            if (request.GetType() == typeof(CalculateDiscountsServiceRequest))
            {
                CalculateDiscountsServiceRequest calculateDiscountsService = (CalculateDiscountsServiceRequest)request;
                priceServiceResponse = await this.ExecuteNextAsync<GetPriceServiceResponse>(request);
                
                if (priceServiceResponse.Transaction.RefundableTenderLines.Count == 0)
                {
                    return priceServiceResponse;
                }
                                
                GetCardRefundChargeCode(request.RequestContext, out string cardRefundChargeCode);
                GetCardRefundProcessingFeePercentage(request.RequestContext, out decimal cardFee);
                GetTenderTypeForRefundCharges(request.RequestContext, out List<string> tenderTypeForRefundCharges);

                if ( !string.IsNullOrEmpty(cardRefundChargeCode) && tenderTypeForRefundCharges.Count > 0)
                {
                    if (priceServiceResponse.Transaction.IsReturnByReceipt)
                    {
                        HashSet<string> supportedTenderTypes = new HashSet<string>(tenderTypeForRefundCharges);
                        List<TenderLine> tenderLists = priceServiceResponse.Transaction.RefundableTenderLines.Where(m => supportedTenderTypes.Contains(m.TenderTypeId)).ToList();
                        List<SalesLine> productReturnableLines = priceServiceResponse.Transaction.ActiveSalesLines.Where(sl => sl.IsReturnLine()).ToList();//.Sum(sl => sl.Price * sl.Quantity);  //RefundableTenderLines.Where(m => supportedTenderTypes.Contains(m.TenderTypeId)).ToList();
                        decimal returnSalesLines = productReturnableLines.Sum(sl => (sl.Price * (sl.QuantityReturnable ?? sl.Quantity)) - sl.DiscountAmount);
                        
                        if (productReturnableLines.Count > 0)
                        {
                            decimal processingFees = returnSalesLines * (Convert.ToDecimal((cardFee)) / 100);
                            ChargeLine ccrChargeLine = priceServiceResponse.Transaction.ChargeLines?.Where(a => a.ChargeCode == cardRefundChargeCode)?.FirstOrDefault() ?? null;
                            if (ccrChargeLine == null)
                            {
                                ChargeLine chargeLine = new ChargeLine();

                                chargeLine.BeginDateTime = DateTimeOffset.MinValue;
                                chargeLine.EndDateTime = DateTimeOffset.MinValue;
                                chargeLine.ChargeLineId = Guid.NewGuid().ToString();
                                chargeLine.ChargeCode = cardRefundChargeCode;
                                chargeLine.CurrencyCode = priceServiceResponse.Transaction.RefundableTenderLines.FirstOrDefault().Currency;
                                chargeLine.ModuleType = ChargeModule.Sales;
                                chargeLine.ModuleTypeValue = Convert.ToInt16(ChargeModule.Sales);
                                chargeLine.CalculatedAmount = Math.Abs(processingFees);
                                chargeLine.Description = cardRefundChargeCode;
                                chargeLine.Quantity = 1;
                                chargeLine.NetAmountPerUnit = Math.Abs(processingFees);
                                priceServiceResponse.Transaction.ChargeLines.Add(chargeLine);
                            }
                            else
                            {
                                ccrChargeLine.CalculatedAmount = Math.Abs(processingFees);
                                ccrChargeLine.NetAmountPerUnit = Math.Abs(processingFees);
                            }
                        }
                    }
                }
                
            }
            return priceServiceResponse;
        }

        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
        }

        private void GetCardRefundChargeCode(RequestContext context, out string CardRefundChargeCode)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            CardRefundChargeCode = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("CardRefundChargeCode").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
        }

        private void GetCardRefundProcessingFeePercentage(RequestContext context, out decimal cardFee)
        {
            cardFee = decimal.Zero;            
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;
            string Value = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("CardRefundFee").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(Value) && decimal.TryParse(Value, out decimal decimalValue))
            {
                cardFee = decimalValue;
            }
        }
        
        private void GetTenderTypeForRefundCharges(RequestContext context, out List<string> tenderTypeForRefundCharges)
        {
            tenderTypeForRefundCharges = new List<string>();

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderTypeForRefund = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("TenderTypeForRefundCharges").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderTypeForRefund))
            {
                tenderTypeForRefundCharges = tenderTypeForRefund.Split(',').ToList();
            }
        }
        
    }
}
