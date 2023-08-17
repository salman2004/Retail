

namespace CDC.Commerce.Runtime.FBRIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

    class CalculateDiscountServiceRequestEx : IRequestHandlerAsync
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
            CalculateDiscountsServiceRequest calculateDiscountsServiceReqeust = (CalculateDiscountsServiceRequest)request;
            GetPriceServiceResponse priceServiceResponse  = await this.ExecuteNextAsync<GetPriceServiceResponse>(calculateDiscountsServiceReqeust);

            if (priceServiceResponse.Transaction.ActiveSalesLines.IsNullOrEmpty())
            {
                return priceServiceResponse;
            }

            foreach (var salesLine in priceServiceResponse.Transaction.ActiveSalesLines.Where(a=> a.TaxLines.Any(b => b.IsPropertyDefined("IsGstTypeProduct"))))
            {
                foreach (var item in salesLine.DiscountLines)
                {
                    item.EffectiveAmount = (salesLine.Price - (salesLine.TaxLines.Sum(a => a.Amount) / salesLine.Quantity)) * (item.EffectivePercentage / 100) * salesLine.Quantity;
                    item.Amount = item.EffectiveAmount / salesLine.Quantity;
                }
                salesLine.DiscountAmount = salesLine.DiscountLines.Sum(a => a.EffectiveAmount);
            }
            priceServiceResponse.Transaction.DiscountAmount = priceServiceResponse.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);

            return priceServiceResponse;
        }
    }
}
