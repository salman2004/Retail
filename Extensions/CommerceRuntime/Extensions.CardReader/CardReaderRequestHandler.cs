using CDC.Commerce.Runtime.CardReader.Entities;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.CardReader
{

    public class CardReaderRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CardReaderRequest)
                };
            }
        }
        
        public async Task<Response> Execute(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Type reqType = request.GetType();
            if (reqType == typeof(CardReaderRequest))
            {
                return await GetCardActivationResponse((CardReaderRequest)request);
            }
            else
            {
                string message = string.Format("Request '{0}' is not supported.", reqType);
                throw new NotSupportedException(message);
            }
        }

        public async Task<CardReaderResponse> GetCardActivationResponse(CardReaderRequest request)
        {
            ThrowIf.Null(request, "request");
            CardReaderResponse activateRFIDCardResponse;

            if (await IsCardBlockedAsync(request))
            {
                throw new CommerceException("Card Error", "The card you are trying to use is blocked.")
                {
                    LocalizedMessage = "The card you are trying to use is blocked. Please contact concerned department."
                };
            }

            GetLoyaltyCardDataRequest getLoyaltyCardDataRequest = new GetLoyaltyCardDataRequest(request.CardNumber);
            getLoyaltyCardDataRequest.RequestContext = request.RequestContext;
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(getLoyaltyCardDataRequest.GetType(), this);
            SingleEntityDataServiceResponse<LoyaltyCard> result = await request.RequestContext.Runtime.ExecuteAsync<SingleEntityDataServiceResponse<LoyaltyCard>>(getLoyaltyCardDataRequest, request.RequestContext, requestHandler, false).ConfigureAwait(false);

            if (result.Entity == null || result.Entity.CustomerAccount == null)
            {
                throw new CommerceException("Card Activation Error", "There was an error with the request")
                {
                    LocalizedMessage = "The provided card number was not found in the system. Please contact concerned department."
                };
            }
            GetCustomerDataRequest customerDataRequest = new GetCustomerDataRequest(result.Entity.CustomerAccount);
            customerDataRequest.RequestContext = request.RequestContext;
            var requestHandlerCustomer = request.RequestContext.Runtime.GetNextAsyncRequestHandler(customerDataRequest.GetType(), this);
            SingleEntityDataServiceResponse<Customer> response = await request.RequestContext.Runtime.ExecuteAsync<SingleEntityDataServiceResponse<Customer>>(customerDataRequest, request.RequestContext, requestHandlerCustomer, false).ConfigureAwait(false);

            if (response.Entity.IdentificationNumber == null || response.Entity == null)
            {
                throw new CommerceException("Card Activation Error", "There was an error with the request")
                {
                    LocalizedMessage = "There was an error finding customer agaisnt the card. Please contact concerned department."
                };
            }

            if (response.Entity.IdentificationNumber.Substring(response.Entity.IdentificationNumber.Length - 6) == request.CNICNumber)
            {
                activateRFIDCardResponse = new CardReaderResponse(true);
                await UpdateCardActivated(request);
            }
            else
            {
                throw new CommerceException("Card Activation Error", "CNIC validation exception")
                {
                    LocalizedMessage = "The provided cnic number is invalid."
                };
            }

            return activateRFIDCardResponse;
        }

        public async Task<int> UpdateCardActivated(CardReaderRequest request)
        {
            ParameterSet parameters = new ParameterSet();
            parameters["@cardNumber"] = request.CardNumber;
            parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var result = await databaseContext.ExecuteStoredProcedureScalarAsync("ext.UpdateCardActivated", parameters, QueryResultSettings.AllRecords).ConfigureAwait(false);
                return result;
            }
        }

        public async Task<bool> IsCardBlockedAsync(CardReaderRequest request)
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

                query.Parameters["@cardNumber"] = request.CardNumber;

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
