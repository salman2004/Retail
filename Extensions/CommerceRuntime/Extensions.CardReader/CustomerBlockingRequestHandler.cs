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
                    typeof(GetLoyaltyCardAffiliationsDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request.GetType() == typeof(GetLoyaltyCardAffiliationsDataRequest))
            {
                GetLoyaltyCardAffiliationsDataRequest affiliationsDataRequest = (GetLoyaltyCardAffiliationsDataRequest)request;
                bool isReturnTransaction = affiliationsDataRequest?.Transaction?.ActiveSalesLines?.Any(sl => sl.IsReturnLine() == true) ?? false;
                if (!affiliationsDataRequest.Transaction.ActiveSalesLines.IsNullOrEmpty() && isReturnTransaction == false && !affiliationsDataRequest.Transaction.IsPropertyDefined("CSDCardNumber") && !affiliationsDataRequest.Transaction.IsPropertyDefined("CDCCardReaderValue"))
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Loyalty Card")
                    {
                        LocalizedMessage = "There was an error reading card. Please contact support for further assitance.",
                        LocalizedMessageParameters = new object[] { }
                    };
                }
                IsCardBlockedAsync(affiliationsDataRequest, out bool isCardBlocked, out bool isRebateCard);
                if(!isCardBlocked)
                {
                    await FilterEmployeeCreditLimitCardAsync(affiliationsDataRequest);
                    if (isRebateCard && request.RequestContext.Runtime.Configuration.IsMasterDatabaseConnectionString)
                    {
                        GetRebateQtyLimitFromHeadQuarters(request.RequestContext, affiliationsDataRequest.LoyaltyCardNumber, affiliationsDataRequest.Transaction);
                    }
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

        public async Task FilterEmployeeCreditLimitCardAsync(GetLoyaltyCardAffiliationsDataRequest request)
        {
            var allowedPaymentMethodForEmployeeCreditLimit = GetRetailConfigurationParameter(request, "AllowedPaymentMethodForEmployeeCreditLimit", request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId);
            var allowedCardForCreditLimit = GetRetailConfigurationParameter(request, "AllowedCardForCreditLimit", request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId);

            if (!string.IsNullOrWhiteSpace(allowedPaymentMethodForEmployeeCreditLimit) && !string.IsNullOrWhiteSpace(allowedCardForCreditLimit))
            {
                var cardTypesForCreditLimit = allowedCardForCreditLimit.ToLower().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (cardTypesForCreditLimit.Any(cardtype => request.Transaction.LoyaltyCardId.ToUpper().StartsWith(cardtype.ToUpper())))
                {
                    var employeeRemainingCreditLimit = await GetEomployeeCreditLimitAsync(request, request.Transaction.CustomerId, request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId);
                    request.Transaction.SetProperty("EmployeeCreditLimit", employeeRemainingCreditLimit.ToString());
                }
            }
        }
        
        private async Task<decimal> GetEomployeeCreditLimitAsync(Request request, string accountNumber, string company)
        {
            InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("GetEmployeeRemainingCreditLimit", accountNumber, company);
            InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
            if ((bool)response.Result[0])
            {
                return Convert.ToDecimal(response.Result[1]);
            }
            else
            {
                throw new Exception(Convert.ToString(response.Result[1]));
            }

        }

        private string GetRetailConfigurationParameter(Request request, string name, string company)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string result = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (name).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
            return result;
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
                    isCardBlocked = Convert.ToBoolean(Convert.ToInt16(entity?.FirstOrDefault()?.GetProperty("CDCISCARDBLOCKED")?.ToString() ?? decimal.Zero.ToString()));
                    isRebateCard = Convert.ToBoolean(Convert.ToInt16(entity?.FirstOrDefault()?.GetProperty("CDCPROTECTMONTHLYCAT")?.ToString() ?? decimal.Zero.ToString()));
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
            string responseFlag = response.Result[0].ToString();
            bool.TryParse(responseFlag, out bool isResponseValid);
            if (isResponseValid)
            {
                string result = Convert.ToString(response.Result[1]);
                transaction.SetProperty("RebateQtyLimit", result);
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
