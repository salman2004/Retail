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
    using Microsoft.Dynamics.Commerce.Runtime.Messages;

    public class SaveSalesTransactionDataRequestTriggerEx : IRequestTriggerAsync
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
                    typeof(SaveSalesTransactionDataRequest)
                    
                };
            }
        }

        public async Task OnExecuted(Request request, Response response)
        {
            await Task.CompletedTask;
        }

        public async Task OnExecuting(Request request)
        {
            if (request is SaveSalesTransactionDataRequest)
            {
                SalesTransaction transaction = (request as SaveSalesTransactionDataRequest).SalesTransaction;

                // Save the transaction sttribute
                request = SaveTransactionAttributes(request.RequestContext, transaction);
            }

            await Task.CompletedTask;
        }

        private Request SaveTransactionAttributes(RequestContext context, SalesTransaction salesTransaction)
        {
            // Validation to check transaction checkout
            if (salesTransaction == null
                || salesTransaction.AmountDue != decimal.Zero
                || salesTransaction.CartType != CartType.Shopping
                || salesTransaction.ExtensibleSalesTransactionType != ExtensibleSalesTransactionType.Sales
                || salesTransaction.IsSales != true
                || string.IsNullOrWhiteSpace(salesTransaction.ReceiptId))
            {
                return new SaveSalesTransactionDataRequest(salesTransaction) { RequestContext = context };
            }
            
            foreach (CommerceProperty commerceProperty in salesTransaction.ExtensionProperties)
            {
                salesTransaction.AttributeValues.Add(new AttributeTextValue() { Name = commerceProperty.Key, TextValue = commerceProperty.Value.StringValue });
            }

            foreach (SalesLine line in salesTransaction.SalesLines)
            {
                foreach (var item in line.ExtensionProperties)
                {
                    line.AttributeValues.Add(new AttributeTextValue() { Name = item.Key, TextValue = item.Value.StringValue });
                }
            }

            return new SaveSalesTransactionDataRequest(salesTransaction) { RequestContext = context };
        }
    }
}
