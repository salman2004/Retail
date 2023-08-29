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
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;

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
            if (request.GetType() == typeof(SaveSalesTransactionDataRequest))
            {
                SalesTransaction transaction = (request as SaveSalesTransactionDataRequest).SalesTransaction;
                if (!request.RequestContext.Runtime.Configuration.IsMasterDatabaseConnectionString)
                {
                    await SaveLastTransactionDateTimeAsync(request.RequestContext, transaction.Id);
                }
                
            }
        }

        public async Task OnExecuting(Request request)
        {
            if (request is SaveSalesTransactionDataRequest)
            {
                SalesTransaction transaction = (request as SaveSalesTransactionDataRequest).SalesTransaction;

                // Save the transaction sttribute
                request = await SaveTransactionAttributes(request.RequestContext, transaction);
            }

            await Task.CompletedTask;
        }


        private async Task SaveLastTransactionDateTimeAsync(RequestContext context, string transacationId)
        {
            ParameterSet parameters = new ParameterSet();
            parameters["@transactionId"] = transacationId;
            parameters["@createdDateTime"] = DateTime.Now.ToString();

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                var result = await databaseContext.ExecuteStoredProcedureNonQueryAsync("[EXT].[SAVELASTTRANSACTIONDATETIME]", parameters, resultSettings: null).ConfigureAwait(false);
            }
        }

        private async  Task<Request> SaveTransactionAttributes(RequestContext context, SalesTransaction salesTransaction)
        {
            // Validation to check transaction checkout
            if (salesTransaction == null
               // || salesTransaction.AmountDue != decimal.Zero
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
            GetCostPriceAsync(context, salesTransaction);

            foreach (SalesLine line in salesTransaction.SalesLines)
            {
                foreach (var item in line.ExtensionProperties)
                {
                    line.AttributeValues.Add(new AttributeTextValue() { Name = item.Key, TextValue = item.Value.StringValue });
                }
            }

            bool.TryParse(salesTransaction.GetProperty("checkLoyaltyLimit")?.ToString() ?? string.Empty, out bool checkLoyaltyLimit);
            if (!salesTransaction.LoyaltyCardId.IsNullOrEmpty() 
                && salesTransaction.IsPropertyDefined("CSDCardBalance")
                && checkLoyaltyLimit)
            {
                await SaveRebateQtyLimitChanges(context, salesTransaction);
            }

            return new SaveSalesTransactionDataRequest(salesTransaction) { RequestContext = context };
        }
        
        private void GetCostPriceAsync(RequestContext context, SalesTransaction transaction)
        {
            List<ExtensionsEntity> entities = new List<ExtensionsEntity>();
            ExtensionsEntity entity = new ExtensionsEntity();
            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"SELECT RECID,(DATAAREAID + '::' +ITEMID + '::' +CONFIGID + '::' +INVENTLOCATIONID + '::' +INVENTCOLORID + '::' +INVENTSTYLEID + '::' +INVENTSIZEID)AS ItemDimensions,CostPrice FROM ext.CDCPRODUCTVARIANTCOSTPRICE C1 WHERE DATAAREAID + ITEMID + CONFIGID + INVENTLOCATIONID + INVENTCOLORID + INVENTSTYLEID + INVENTSIZEID IN({string.Join(",", transaction.SalesLines.Select(sl => "'" + context.GetChannelConfiguration().InventLocationDataAreaId + sl.ItemId + sl.Variant.ConfigId + sl.InventoryLocationId + sl.Variant.ColorId + sl.Variant.StyleId + sl.Variant.SizeId + "'"))})";
                
                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();

                    foreach (var sl in transaction.SalesLines)
                    {
                        entity = entities.Where(a => a.GetProperty("ItemDimensions").ToString() == context.GetChannelConfiguration().InventLocationDataAreaId + "::" + sl.ItemId + "::" + sl.Variant.ConfigId + "::" + sl.InventoryLocationId + "::" + sl.Variant.ColorId + "::" + sl.Variant.StyleId + "::" + sl.Variant.SizeId).OrderBy(b => b.GetProperty("RECID")).FirstOrDefault();
                        if (entity != null)
                        {
                            sl.SetProperty("CostPrice", entity.GetProperty("CostPrice").ToString());
                        }
                        else
                        {
                            sl.SetProperty("CostPrice", decimal.Zero.ToString());
                        }
                            
                    }
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.AxGenericErrorEvent(ex.Message);
                }
            }
            
        }

        private async Task SaveRebateQtyLimitChanges(RequestContext context, SalesTransaction transaction)
        {
            string categoryQuantityLimitedUpdated = transaction.GetProperty("SaveRebateQtyLimit")?.ToString() ?? string.Empty;
            if (!categoryQuantityLimitedUpdated.IsNullOrEmpty() && context.Runtime.Configuration.IsMasterDatabaseConnectionString)
            {
                InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("SetRebateQtyLimitByLoyaltyCardId", transaction.LoyaltyCardId, categoryQuantityLimitedUpdated, context.GetChannelConfiguration().InventLocationDataAreaId);
                InvokeExtensionMethodRealtimeResponse response = await context.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                string responseFlag = response.Result[0].ToString();
                bool.TryParse(responseFlag, out bool isResponseValid);                
            }
        }
        
    }
}
