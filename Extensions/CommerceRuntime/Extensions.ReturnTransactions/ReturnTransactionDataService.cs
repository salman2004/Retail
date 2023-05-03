namespace CDC.Commerce.Runtime.ReturnTransactions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

    public class ReturnTransactionDataService : IRequestHandlerAsync
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
                    typeof(AddCartLinesRequest)
                    
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            
            try
            {
                List<string> transactionIdsNotAllowed = new List<string>(); 
                DateTime transcationDateTime = DateTime.MinValue;
                string errorMessage = string.Empty;
                var channelConfigs = request.RequestContext.GetChannelConfiguration();
                AddCartLinesRequest returnTransaction = (AddCartLinesRequest)request;
                if (returnTransaction != null && !returnTransaction.CartLines.Where(cl => cl.ReturnTransactionId != string.Empty).IsNullOrEmpty())
                {
                    GetAllowedNumberOfDays(request.RequestContext, out ExtensionsEntity entity);
                    int.TryParse(entity?.GetProperty("AllowedDaysReturn")?.ToString() ?? decimal.Zero.ToString(), out int AllowedDaysReturn);
                    GetReturnTransactionCreatedDateTime(request.RequestContext, returnTransaction.CartLines.Where(cl => cl.ReturnTransactionId != string.Empty).Select(cl => cl.ReturnTransactionId).ToList(), out List<ExtensionsEntity> entities);
                    foreach (var item in entities)
                    {
                        DateTime.TryParse(item.GetProperty("CREATEDDATETIME")?.ToString() ?? string.Empty, out transcationDateTime);
                        int dateDifference = (int) (DateTime.Now - transcationDateTime).TotalDays;
                        if (dateDifference > AllowedDaysReturn)
                        {
                            transactionIdsNotAllowed.Add(item.GetProperty("TRANSACTIONID").ToString());
                        }
                    }

                    if (!transactionIdsNotAllowed.IsNullOrEmpty())
                    {
                        if (!request.RequestContext.Runtime.Configuration.IsMasterDatabaseConnectionString)
                        {
                            throw new CommerceException("Microsoft_Dynamics_Commerce", string.Format("Transaction {0} could not be returned. Unable to establish connection with HQ.", transactionIdsNotAllowed.FirstOrDefault()))
                            {
                                LocalizedMessage = string.Format("Transaction {0} could not be returned. Unable to establish connection with HQ.", transactionIdsNotAllowed.FirstOrDefault()),
                                LocalizedMessageParameters = new object[] { }
                            };
                        }
                        InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("ValidateReturnTransaction", string.Join(",", transactionIdsNotAllowed.Select(sl => sl.ToString())), channelConfigs.InventLocationDataAreaId);
                        InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                        if (!(bool)response.Result[0])
                        {
                            errorMessage += Convert.ToString(response.Result[1]);
                        }
                    }
                    else
                    {
                        return await this.ExecuteNextAsync<Response>(request);
                    }
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        throw new CommerceException("Return Transaction", errorMessage)
                        {
                            LocalizedMessage = errorMessage,
                            LocalizedMessageParameters = new object[] { }
                        };
                    }
                }
                else
                {
                    return await this.ExecuteNextAsync<Response>(request);
                }
            }
            catch (Exception ex)
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce", string.Format("{0}", ex.Message))
                {
                    LocalizedMessage = string.Format("{0}", ex.Message),
                    LocalizedMessageParameters = new object[] { }
                };
            }
            return await this.ExecuteNextAsync<Response>(request);
        }

        public void GetAllowedNumberOfDays(RequestContext context, out ExtensionsEntity entity)
        {
            entity = new ExtensionsEntity();

            if (context.GetChannelConfiguration().RetailFunctionalityProfileId.IsNullOrEmpty())
            {
                return;
            }

            SqlQuery query = new SqlQuery();
            query.QueryString = @"SELECT TOP 1 AllowedDaysReturn FROM EXT.RETAILFUNCTIONALITYPROFILE WHERE PROFILEID = @profileId";
            query.Parameters["@profileId"] = context.GetChannelConfiguration().RetailFunctionalityProfileId;
            try
            {
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    entity = databaseContext.ReadEntity<ExtensionsEntity>(query).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                entity = new ExtensionsEntity();
            }
        }
        
        public void GetReturnTransactionCreatedDateTime(RequestContext context, List<string> TransactionIds, out List<ExtensionsEntity> entities)
        {
            if (TransactionIds.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
            }

            entities = null;

            SqlQuery query = new SqlQuery();
            query.QueryString = $@"SELECT TOP 1 CREATEDDATETIME,TRANSACTIONID FROM AX.RETAILTRANSACTIONTABLE WHERE TRANSACTIONID IN ({string.Join(",", TransactionIds.Select(sl => "'" + sl.ToString() + "'"))})";
            
            try
            {
                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                }
            }
            catch (Exception)
            {
                entities = new List<ExtensionsEntity>();
            }
        }
    }
}
