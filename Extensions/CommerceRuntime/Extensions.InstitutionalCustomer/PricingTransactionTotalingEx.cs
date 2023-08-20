namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using PE = Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine;

    class PricingTransactionTotalingEx : PE.IPricingTransactionTotalingHelper
    {
        protected RequestContext RequestContext { get; private set; }

        public PricingTransactionTotalingEx(RequestContext requestContext)
        {
            RequestContext = requestContext;
        }

        public PE.DiscountData.TransactionTotals CalculateTransactionTotals(SalesTransaction transaction)
        {
            CalculateSalesTransactionServiceRequest request = new CalculateSalesTransactionServiceRequest(transaction, CalculationModes.Taxes | CalculationModes.Totals | CalculationModes.Deposit | CalculationModes.AmountDue | CalculationModes.Prices );
           
         //   CalculateSalesTransactionServiceRequest request = new CalculateSalesTransactionServiceRequest(transaction, CalculationModes.All);

            transaction = RequestContext.ExecuteAsync<CalculateSalesTransactionServiceResponse>(request).Result.Transaction;
            return new PE.DiscountData.TransactionTotals
            {
                TotalAmount = transaction.TotalAmount
            };
        }
    }
}
