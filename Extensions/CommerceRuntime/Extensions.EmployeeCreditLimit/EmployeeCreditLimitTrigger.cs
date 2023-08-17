
namespace CDC.Commerce.Runtime.EmployeeCreditLimit
{
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class EmployeeCreditLimitTrigger : IRequestTriggerAsync
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
                    typeof(CheckoutCartRequest)
                };
            }
        }

        public async Task OnExecuted(Request request, Response response)
        {
            await Task.CompletedTask;
        }

        public async Task OnExecuting(Request request)
        {
            if (request is CheckoutCartRequest)
            {
                try
                {
                    var checkoutCartRequest = (CheckoutCartRequest)request;
                    var getCartRequest = new GetCartRequest(
                        new CartSearchCriteria(checkoutCartRequest.CartId, checkoutCartRequest.CartVersion),
                        new QueryResultSettings(
                        new PagingInfo(1)));

                    var getCartResponse = request.RequestContext.ExecuteAsync<GetCartResponse>(getCartRequest).Result;

                    if (getCartResponse != null && getCartResponse.Carts?.FirstOrDefault() != null)
                    {
                        ChannelConfiguration channelConfigs = request.RequestContext.GetChannelConfiguration();

                        var allowedPaymentMethodForEmployeeCreditLimit = GetRetailConfigurationParameter(request, "AllowedPaymentMethodForEmployeeCreditLimit", channelConfigs.InventLocationDataAreaId);
                        var allowedCardForCreditLimit = GetRetailConfigurationParameter(request, "AllowedCardForCreditLimit", channelConfigs.InventLocationDataAreaId);

                        if (!string.IsNullOrWhiteSpace(allowedPaymentMethodForEmployeeCreditLimit) && !string.IsNullOrWhiteSpace(allowedCardForCreditLimit))
                        {
                            var paymentMethodsForEmployeeCreditLimit = allowedPaymentMethodForEmployeeCreditLimit.ToLower().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            var cardTypesForCreditLimit = allowedCardForCreditLimit.ToLower().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            var cart = getCartResponse.Carts.FirstOrDefault();
                            var transaction = getCartResponse.Transactions.FirstOrDefault();
                            var loyalityCard = cart?.LoyaltyCardId;
                            var accountNumber = cart?.CustomerId;

                            foreach (var tenderLine in cart.TenderLines.Where(x=>x.IsVoided == false).ToList())
                            {
                                var tenderTypeId = tenderLine?.TenderTypeId;
                                var orderAmount = tenderLine?.Amount;

                                if (paymentMethodsForEmployeeCreditLimit.Any(x => x == tenderTypeId) && cardTypesForCreditLimit.Any(cardtype => loyalityCard.ToUpper().StartsWith(cardtype.ToUpper())))
                                {
                                    string employeeRemainingCreditLimit = GetEomployeeCreditLimitAsync(transaction);
                                    if (!employeeRemainingCreditLimit.IsNullOrEmpty())
                                    {
                                        var employeeRemainingCreditLimitValue = Convert.ToDecimal(employeeRemainingCreditLimit ?? decimal.Zero.ToString());
                                        if (orderAmount > employeeRemainingCreditLimitValue)
                                        {
                                            throw new Exception($"Only amount '{employeeRemainingCreditLimit}'is available for credit. Please pay remaining amount using another tender.");
                                        }
                                        else
                                        {
                                            var newCreditLimit = employeeRemainingCreditLimitValue - orderAmount;
                                            await InsertUpdateEomployeeCreditLimitAsync(request, channelConfigs.InventLocationDataAreaId, accountNumber, transaction.Id, channelConfigs.ChannelNaturalId, (decimal)orderAmount, loyalityCard, employeeRemainingCreditLimitValue, (decimal)newCreditLimit);
                                        }
                                    }
                                } 
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Custom error")
                    {
                        LocalizedMessage = exception.Message

                    };
                }

            }

            await Task.CompletedTask;
        }

        private string GetRetailConfigurationParameter(Request request, string name, string company)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string result = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (name).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
            return result;
        }

        private string GetEomployeeCreditLimitAsync(SalesTransaction transaction)
        {
            return transaction?.GetProperty("EmployeeCreditLimit")?.ToString() ?? string.Empty;            
        }

        private async Task<bool> InsertUpdateEomployeeCreditLimitAsync(Request request, string company, string accountNumber, string transactionId, string storeNumber, decimal tenderAmount, string cardNumber, decimal previousCreditLimit, decimal newCreditLimit)
        {
            InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("InsertUpdateEmployeeRemainingCreditLimit", company, accountNumber, transactionId, storeNumber, tenderAmount, cardNumber, previousCreditLimit, newCreditLimit);
            InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);

            if ((bool)response.Result[0])
            {
                return true;
            }
            else
            {
                throw new Exception(Convert.ToString(response.Result[1]));
            }
        }
    }


}
