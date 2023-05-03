using CDC.Commerce.Runtime.AskariCardBinNumberVerification.Model;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.AskariCardBinNumberVerification
{
    public class AskariCardBinVerificationRequestHandler : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(AskariCardBinVerificationRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");
            Type reqType = request.GetType();

            if (reqType == typeof(AskariCardBinVerificationRequest))
            {
                AskariCardBinVerificationRequest dateValidationRequest = (AskariCardBinVerificationRequest)request;
                string cardNumber = dateValidationRequest.CardNumber.Replace("-", string.Empty);
                //await SaveInfoCodeLine(dateValidationRequest.TransactionId, request.RequestContext, cardNumber, GetConfigurationParameters(request.RequestContext, "AskariCardInfoCode"));
                return new AskariCardBinVerificationResponse(ValidateBinNumber(dateValidationRequest));
            }
            else
            {
                string message = string.Format("Request '{0}' is not supported.", reqType);
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool ValidateBinNumber(AskariCardBinVerificationRequest request)
        {
            try
            {
                string askariCardBinNumber = GetConfigurationParameters(request.RequestContext, "AskariCardBinNumber");
                if (askariCardBinNumber != string.Empty)
                {
                    foreach (var item in askariCardBinNumber.Split(','))
                    {
                        string cardNumberSubStr = request.CardNumber.Substring(0, item.Length);
                        if (item.Equals(cardNumberSubStr))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Askari card error")
                {
                    LocalizedMessage = string.Format("{0}", ex.Message),
                    LocalizedMessageParameters = new object[] { }
                };
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetConfigurationParameters(RequestContext context, string key)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            RetailConfigurationParameter paramter = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (key).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();

            return paramter?.Value ?? string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="context"></param>
        /// <param name="infoCodetext"></param>
        /// <param name="infoCodeId"></param>
        /// <returns></returns>
        public static async Task SaveInfoCodeLine(string transactionId, RequestContext context, string infoCodetext, string infoCodeId)
        {

            ReasonCodeLine reasonCodeLine = new ReasonCodeLine();
            reasonCodeLine.Amount = 0;
            reasonCodeLine.InputType = ReasonCodeInputType.Text;
            reasonCodeLine.ReasonCodeId = infoCodeId;
            reasonCodeLine.TransactionId = transactionId;
            reasonCodeLine.Information = infoCodetext;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<SalesTransaction> GetCurrentSalesTransactionAsync(string transactionId, RequestContext context)
        {
            CartSearchCriteria cartSearchCriteria = new CartSearchCriteria(transactionId);
            GetCartRequest getCartRequest = new GetCartRequest(cartSearchCriteria, QueryResultSettings.SingleRecord);
            getCartRequest.RequestContext = context;
            GetCartResponse response = await context.Runtime.ExecuteAsync<GetCartResponse>(getCartRequest, context);
            return response.Transactions.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Cart> GetCurrentCartAsync(string transactionId, RequestContext context)
        {
            CartSearchCriteria cartSearchCriteria = new CartSearchCriteria(transactionId);
            GetCartRequest getCartRequest = new GetCartRequest(cartSearchCriteria, QueryResultSettings.SingleRecord);
            getCartRequest.RequestContext = context;
            GetCartResponse response = await context.Runtime.ExecuteAsync<GetCartResponse>(getCartRequest, context);
            return response.Carts.FirstOrDefault();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Response> UpdateCart(Cart cart, RequestContext context)
        {
            UpdateCartRequest updateCartRequest = new UpdateCartRequest(cart);
            updateCartRequest.RequestContext = context;
            return await context.Runtime.ExecuteAsync<Response>(updateCartRequest, context);
        }
    }
}
        
