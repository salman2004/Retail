using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            CalculateTaxServiceResponse serviceResponse = (CalculateTaxServiceResponse) await ExecuteBaseRequestAsync(calculateTax);

            foreach (var salesLine in calculateTax.Transaction.ActiveSalesLines)
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

                        foreach (var item in salesLine.DiscountLines)
                        {
                            item.EffectiveAmount = (salesLine.AgreementPrice - (taxLines.Sum(a => a.Amount) / salesLine.Quantity)) * (item.EffectivePercentage / 100) * salesLine.Quantity;
                            item.Amount = item.EffectiveAmount;
                            item.DiscountCost = item.EffectiveAmount;
                        }
                        salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
                        salesLine.DiscountAmountWithoutTax = salesLine.DiscountAmount;
                        salesLine.PeriodicDiscount = salesLine.DiscountAmount;
                        salesLine.PeriodicPercentageDiscount = salesLine.DiscountAmount;
                        salesLine.TotalPercentageDiscount = salesLine.DiscountAmount;
                    }
                }
                
            }

            calculateTax.Transaction.DiscountAmount = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);
            calculateTax.Transaction.DiscountAmountWithoutTax = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);
            calculateTax.Transaction.PeriodicDiscountAmount = calculateTax.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);

            return serviceResponse;
        }
        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
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
