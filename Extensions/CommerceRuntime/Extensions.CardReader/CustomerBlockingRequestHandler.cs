using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using CDC.Commerce.Runtime.CardReader.Entities;
using System.Linq;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;

namespace CDC.Commerce.Runtime.CardReader
{
    public class CustomerBlockingRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetLoyaltyCardAffiliationsDataRequest),
                    typeof(InsertLoyaltyCardDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {

            if (request.GetType() == typeof(GetLoyaltyCardAffiliationsDataRequest))
            {
                GetLoyaltyCardAffiliationsDataRequest affiliationsDataRequest = (GetLoyaltyCardAffiliationsDataRequest)request;
                IsCardBlockedAsync(affiliationsDataRequest, out bool isCardBlocked, out bool isRebateCard);
                if(!isCardBlocked)
                {
                    //if (isRebateCard && request.RequestContext.Runtime.Configuration.IsMasterDatabaseConnectionString)
                    //{
                    //    GetRebateQtyLimitFromHeadQuarters(request.RequestContext, affiliationsDataRequest.LoyaltyCardNumber, affiliationsDataRequest.Transaction);
                    //}
                    return await ExecuteBaseRequestAsync(request);
                }
                else
                {
                    throw new CommerceException("Card Error", "The card you are trying to use is blocked.")
                    {
                        LocalizedMessage = "The card you are trying to use is blocked. Please contact concerned department."
                    };
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
        }

        public void IsCardBlockedAsync(GetLoyaltyCardAffiliationsDataRequest request, out bool isCardBlocked, out bool isRebateCard)
        {
            isCardBlocked = false;
            isRebateCard = false;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select R1.CDCISCARDBLOCKED, R3.CDCPROTECTMONTHLYCAT from ext.RETAILLOYALTYCARD R1 join ax.RetailLoyaltyCardTier R2 on R2.LOYALTYCARD = R1.RECID join ext.RETAILAFFILIATION R3 on R3.RECID = R2.AFFILIATION where R1.CARDNUMBER = @cardNumber";
                query.Parameters["@cardNumber"] = request.LoyaltyCardNumber;
            
                try
                {
                    List<ExtensionsEntity> entity = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                    isCardBlocked = Convert.ToBoolean(entity?.FirstOrDefault()?.GetProperty("CDCISCARDBLOCKED").ToString() ?? Boolean.FalseString);
                    isRebateCard = Convert.ToBoolean(entity?.FirstOrDefault()?.GetProperty("CDCPROTECTMONTHLYCAT").ToString() ?? Boolean.FalseString);
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.AxGenericErrorEvent($"Reding loyalty card config failed. {ex?.Message ?? string.Empty}");
                }
            }
        }

        public async void GetRebateQtyLimitFromHeadQuarters(RequestContext context, string loyaltyCardNumber, SalesTransaction transaction)
        {
            InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("GetRebateQtyLimitByLoyaltyCardId", loyaltyCardNumber, context.GetChannelConfiguration().InventLocationDataAreaId);
            InvokeExtensionMethodRealtimeResponse response = await context.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
            if (!(bool)response.Result[0])
            {
                transaction.SetProperty("RebateQtyLimit", Convert.ToString(response.Result[1]));
            }
            else
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", Convert.ToString(response.Result[1]))
                {
                    LocalizedMessage = Convert.ToString(response.Result[1]),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }
    }
}
