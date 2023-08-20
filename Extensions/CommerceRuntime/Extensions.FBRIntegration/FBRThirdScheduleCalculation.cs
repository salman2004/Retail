using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.FBRIntegration
{
    public class FBRThirdScheduleCalculation : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CalculateTaxServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {

            CalculateTaxServiceRequest calculateTax = (CalculateTaxServiceRequest)request;
            CalculateTaxServiceResponse serviceResponse = await this.ExecuteNextAsync<CalculateTaxServiceResponse>(request);

            foreach (var salesLine in serviceResponse.Transaction.ActiveSalesLines)
            {
                if (!salesLine.IsReturnLine())
                {
                    GetMarginCapOnProductAndProductCategory(request.RequestContext, salesLine.ItemId, salesLine.Variant.VariantId, request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId, out decimal maximumRetailPrice, out int gstType);

                    if (gstType == 1)
                    {
                        Collection<TaxLine> taxLines = new Collection<TaxLine>();
                        foreach (var taxLine in salesLine.TaxLines)
                        {
                            if (!taxLine.IsIncludedInPrice)
                            {
                                taxLine.Amount = decimal.Round((taxLine.Percentage / 100 * maximumRetailPrice) * salesLine.Quantity, 2, MidpointRounding.AwayFromZero);
                                taxLine.TaxBasis = maximumRetailPrice * salesLine.Quantity;
                            }
                            else
                            {
                                taxLine.Amount = decimal.Round(((maximumRetailPrice * taxLine.Percentage) / (taxLine.Percentage + 100)) * salesLine.Quantity, 2, MidpointRounding.AwayFromZero);
                                taxLine.TaxBasis = maximumRetailPrice * salesLine.Quantity;
                            }
                            taxLines.Add(taxLine);

                        }

                        salesLine.NetAmountWithoutTax = maximumRetailPrice;

                        salesLine.TaxAmount = taxLines.Sum(a => a.Amount);
                        salesLine.TaxLines = taxLines;

                        
                       /*
                        GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);
                        GetTenderDiscountValue(request.RequestContext, out decimal maxDiscount);

                        decimal totalTenderTypeDiscountAmount = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.DiscountLines.Where(b => b.DiscountLineType == DiscountLineType.TenderTypeDiscount && offerIds.Contains(b.OfferId)).Sum(c => c.EffectiveAmount));

                        if (totalTenderTypeDiscountAmount < maxDiscount)
                        {
                            foreach (var item in salesLine.DiscountLines.Where(a => !offerIds.Contains(a.OfferId)))
                            {
                                if (!(offerIds.Contains(item.OfferId) && item.EffectiveAmount == maxDiscount || item.EffectivePercentage == 0 && item.EffectiveAmount > 0))
                                {
                                    item.EffectiveAmount = (salesLine.Price - (taxLines.Sum(a => a.Amount) / salesLine.Quantity)) * (item.EffectivePercentage / 100) * salesLine.Quantity;
                                    item.Amount = item.EffectiveAmount;
                                }
                            }

                            foreach (var item in salesLine.DiscountLines.Where(a => offerIds.Contains(a.OfferId)))
                            {
                                item.EffectiveAmount = ((((salesLine.Price * salesLine.Quantity) - (salesLine.DiscountLines.Where(d => !offerIds.Contains(d.OfferId)).Sum(a => a.EffectiveAmount))) * (item.Percentage / 100)));
                                item.Amount = item.EffectiveAmount;   
                            }
                            
                        }
                        else
                        {
                            foreach (var item in salesLine.DiscountLines.Where(a => !offerIds.Contains(a.OfferId)))
                            {
                                item.EffectiveAmount = (salesLine.Price - (taxLines.Sum(a => a.Amount) / salesLine.Quantity)) * (item.EffectivePercentage / 100) * salesLine.Quantity;
                                item.Amount = item.EffectiveAmount;
                            }
                        }

                        salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                        salesLine.DiscountAmountWithoutTax = salesLine.DiscountLines.Sum(a => a.DiscountCost);
                        salesLine.PeriodicDiscount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.PeriodicDiscount).Sum(line => line.EffectiveAmount);
                        salesLine.TenderDiscountAmount = salesLine.DiscountLines.Where(a => a.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(line => line.EffectiveAmount);
                    
                        */
                        }
                    
                }                
            }
           var checkVale = serviceResponse.Transaction.GetProperty("isTenderDiscount").ToString();
            
           if (checkVale.ToLower() == "false")
            { 
            CalculateDiscountsServiceRequest calculateDiscountsRequest = new CalculateDiscountsServiceRequest(serviceResponse.Transaction);
            GetPriceServiceResponse calculateDiscountResponse  = await request.RequestContext.Runtime.ExecuteAsync<GetPriceServiceResponse>(calculateDiscountsRequest,request.RequestContext).ConfigureAwait(false);
            
            serviceResponse.Transaction.SalesLines = calculateDiscountResponse.Transaction.SalesLines;
                serviceResponse.Transaction.SetProperty("isTenderDiscount",false);
            }
            

            /*
            calculateTax.Transaction.DiscountAmount = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);
            calculateTax.Transaction.DiscountAmountWithoutTax = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmountWithoutTax);
            calculateTax.Transaction.PeriodicDiscountAmount = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.PeriodicDiscount);
            calculateTax.Transaction.TenderDiscountAmount = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.TenderDiscountAmount);
            */

           // foreach(var  salesLine in serviceResponse.Transaction.SalesLines)
           // serviceResponse.Transaction.NetAmount = serviceResponse.Transaction.SalesLines.Where(p=>!p.IsVoided).Sum(p=>p.NetAmount);
           // serviceResponse.Transaction.NetAmountWithoutTax = serviceResponse.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(p => p.NetAmountWithoutTax);
           // serviceResponse.Transactionfor.NetAmountWithoutTax = serviceResponse.Transaction.SalesLines.Where(p => !p.IsVoided).Sum(p => p.NetAmountWithAllInclusiveTax);

            return serviceResponse;
        }
        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
        }

        private void GetTenderDiscountOfferIds(RequestContext context, out List<string> offerIds)
        {
            offerIds = new List<string>();

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountOfferIds = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTHEADER").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountOfferIds))
            {
                offerIds = tenderDiscountOfferIds.Split(';').ToList();
            }
        }

        private void GetTenderDiscountValue(RequestContext context, out decimal maxDiscount)
        {
            maxDiscount = decimal.Zero;

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTVALUE").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountValue) && decimal.TryParse(tenderDiscountValue, out decimal value))
            {
                maxDiscount = value;
            }
        }

        public void GetMarginCapOnProductAndProductCategory(RequestContext requestContext, String itemId, string variantId, string dataAreaId, out decimal maximumRetailPrice, out int gstType)
        {
            maximumRetailPrice = decimal.Zero;
            gstType = 0;

            ParameterSet parameters = new ParameterSet();
            parameters["@itemId"] = itemId;
            parameters["@variantId"] = variantId;
            parameters["@dataAreaId"] = dataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(requestContext))
            {
                try
                {
                    var result = databaseContext.ExecuteStoredProcedure<ExtensionsEntity>("ext.GETTHIRDSCHEDULEPRODUCTINFO", parameters, QueryResultSettings.AllRecords);
                    gstType = Convert.ToInt32(Convert.ToString(result.FirstOrDefault()?.GetProperty("GSTTYPE") ?? decimal.Zero));
                    maximumRetailPrice = Convert.ToDecimal(Convert.ToString(result.FirstOrDefault()?.GetProperty("MAXIMUMRETAILPRICE") ?? decimal.Zero));

                }
                catch (Exception)
                {
                    maximumRetailPrice = decimal.Zero;
                    gstType = 0;
                }
            }
        }
    
        
        }
}
