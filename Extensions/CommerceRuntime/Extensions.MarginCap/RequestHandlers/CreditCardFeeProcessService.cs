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
                priceServiceResponse = (GetPriceServiceResponse)await ExecuteBaseRequestAsync(request);
                
                if (calculateDiscountsService.Transaction.RefundableTenderLines.Count == 0)
                {
                    return priceServiceResponse;
                }
                                
                GetCardRefundChargeCode(request.RequestContext, out string cardRefundChargeCode);
                GetCardRefundProcessingFeePercentage(request.RequestContext, out decimal cardFee);
                GetTenderTypeForRefundCharges(request.RequestContext, out List<string> tenderTypeForRefundCharges);

                if ( !string.IsNullOrEmpty(cardRefundChargeCode) && tenderTypeForRefundCharges.Count > 0)
                {
                    if (calculateDiscountsService.Transaction.IsReturnByReceipt)
                    {
                        HashSet<string> supportedTenderTypes = new HashSet<string>(tenderTypeForRefundCharges);
                        List<TenderLine> tenderList = calculateDiscountsService.Transaction.RefundableTenderLines.Where(m => supportedTenderTypes.Contains(m.TenderTypeId)).ToList();
                        decimal sum = tenderList.Sum(a => a.Amount);

                        //List<TenderLine> tenderList = calculateDiscountsService.Transaction.RefundableTenderLines.Where(a => Convert.ToInt32(a.TenderTypeId) == 3 || Convert.ToInt32(a.TenderTypeId) == 4 || Convert.ToInt32(a.TenderTypeId) == 5 || Convert.ToInt32(a.TenderTypeId) == 1).ToList();
                        //foreach (var item in tenderList)
                        //{
                        if (tenderList.Count > 0)
                        {
                            //calculateDiscountsService.Transaction.ChargeLines.RemoveAt(calculateDiscountsService.Transaction.ChargeLines.in);//a => a.ChargeCode == cardRefundChargeCode);
                            decimal processingFees = tenderList.Sum(a => a.Amount) * Convert.ToDecimal((cardFee)) / 100;
                            if (!calculateDiscountsService.Transaction.ChargeLines.Any(a => a.ChargeCode == cardRefundChargeCode))
                            {
                                ChargeLineOverride chargeLineOverride = new ChargeLineOverride();
                                chargeLineOverride.OriginalAmount = 0;
                                chargeLineOverride.OverrideAmount = processingFees;
                                chargeLineOverride.OverrideReasonDescription = cardRefundChargeCode;
                                chargeLineOverride.UserId = request.RequestContext.GetPrincipal().ExternalIdentityId;
                                chargeLineOverride.OverrideDateTime = DateTime.Now;

                                ChargeLine chargeLine = new ChargeLine();

                                chargeLine.BeginDateTime = DateTimeOffset.MinValue;
                                chargeLine.EndDateTime = DateTimeOffset.MinValue;
                                chargeLine.ChargeLineId = Guid.NewGuid().ToString();
                                chargeLine.ChargeCode = cardRefundChargeCode;
                                chargeLine.CurrencyCode = tenderList.First().Currency;
                                chargeLine.ModuleType = ChargeModule.Sales;
                                chargeLine.ModuleTypeValue = Convert.ToInt16(ChargeModule.Sales);
                                chargeLine.CalculatedAmount = processingFees;
                                chargeLine.Description = cardRefundChargeCode;
                                chargeLine.Quantity = 1;
                                chargeLine.NetAmountPerUnit = processingFees;
                                chargeLine.ChargeLineOverrides.Add(chargeLineOverride);
                                calculateDiscountsService.Transaction.ChargeLines.Add(chargeLine);


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
