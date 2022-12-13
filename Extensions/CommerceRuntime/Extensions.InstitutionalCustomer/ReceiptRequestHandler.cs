using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    public class ReceiptRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetSalesOrderDetailsByTransactionIdServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
            {
            ThrowIf.Null(request, "request");

            if (request.GetType() == typeof(GetSalesOrderDetailsByTransactionIdServiceRequest))
            {
                GetSalesOrderDetailsByTransactionIdServiceRequest getSalesOrderDetails = (GetSalesOrderDetailsByTransactionIdServiceRequest)request;
                GetSalesOrderDetailsServiceResponse response =await this.ExecuteNextAsync<GetSalesOrderDetailsServiceResponse>(getSalesOrderDetails);

                if (response.SalesOrder== null)
                {
                    return response;
                }

                if (response.SalesOrder.LoyaltyCardId == (response.SalesOrder.AttributeValues.Where(a=>a.Name == "CSDCardNumber")?.FirstOrDefault()?.ToString()?.Trim() ?? null))
                {
                    response.SalesOrder.SalesLines = new Collection<SalesLine>(response.SalesOrder.SalesLines
                        .OrderByDescending(a => Convert.ToDecimal( (a.AttributeValues.Where(z=>z.Name == "CDCTOPONCART")?.FirstOrDefault()?.ToString()?.Trim() == string.Empty) ? decimal.Zero.ToString() : a.AttributeValues.Where(z => z.Name == "CDCTOPONCART")?.FirstOrDefault()?.ToString()?.Trim()))
                        .ThenByDescending(x => Convert.ToDecimal( (x.AttributeValues.Where(z => z.Name == "CDCPRICINGPRIORITY")?.FirstOrDefault()?.ToString()?.Trim() ==string.Empty ) ? decimal.Zero.ToString() : x.AttributeValues.Where(z => z.Name == "CDCPRICINGPRIORITY")?.FirstOrDefault()?.ToString()?.Trim()))
                        .ThenByDescending(y => Convert.ToDecimal( (y.AttributeValues.Where(z => z.Name == "GrossProfit")?.FirstOrDefault()?.ToString()?.Trim() == string.Empty) ? decimal.Zero.ToString() : y.AttributeValues.Where(z => z.Name == "GrossProfit")?.FirstOrDefault()?.ToString()?.Trim())).ToList());
                }
                return response;
            }
            else
            {
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }

        }
        
    }
}
