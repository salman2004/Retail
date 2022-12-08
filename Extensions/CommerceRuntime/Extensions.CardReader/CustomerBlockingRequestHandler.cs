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
                if (!await IsCardBlockedAsync(affiliationsDataRequest))
                {
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

        public async Task<bool> IsCardBlockedAsync(GetLoyaltyCardAffiliationsDataRequest request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCISCARDBLOCKED"),
                    From = "RETAILLOYALTYCARD",
                    Where = "CARDNUMBER = @cardNumber",
                    OrderBy = "CDCISCARDBLOCKED"
                };

                query.Parameters["@cardNumber"] = request.LoyaltyCardNumber;

                var result = await databaseContext.ReadEntityAsync<DBEntity>(query).ConfigureAwait(false);
                if (result.IsNullOrEmpty() || result.Count() == 0)
                {
                    return false;
                }
                return Convert.ToBoolean(result.FirstOrDefault().GetProperty("CDCISCARDBLOCKED"));
            }
        }

    }
}
