using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using CDC.Commerce.Runtime.EhsasProgram.Model;
using CDC.Commerce.Runtime.EhsasProgram.ResendOtpMode;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Framework.Serialization;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataManagers;
using Microsoft.Dynamics.Commerce.Runtime.Services;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram
{
    public class EhsasProgramOtpRequestHandler : IRequestHandlerAsync
    {
        private const string SUCCESS = "Success";

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(VerifyEhsasProgramOtpRequest),
                    typeof(ResendOtpRequest)
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
            if (reqType == typeof(VerifyEhsasProgramOtpRequest))
            {
                var result = await GetEhsasProgramOtpResponseAsync((VerifyEhsasProgramOtpRequest)request);
                return result;

            }
            else if (reqType == typeof(ResendOtpRequest))
            {
                return await GetResendOtpResponseAsync((ResendOtpRequest)request);
            }
            else
            {
                string message = string.Format("Request '{0}' is not supported.", reqType);
                throw new NotSupportedException(message);
            }
        }

        private async Task<VerifyEhsasProgramOtpResponse> GetEhsasProgramOtpResponseAsync(VerifyEhsasProgramOtpRequest request)
        {
            string address = EhsasProgramHelper.GetEhsasProgramConfigurations(request.RequestContext).GetProperty("CDCEHSASPROGRAMSUBSIDYPAYMENTLINK")?.ToString() ?? string.Empty;
            string body = JsonHelper.Serialize(PrepareOtpRequestBody(request));
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, body, EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-SubsidyPayRequest").Value);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AuthToken);

            HttpResponseMessage response = EhsasProgramHelper.GetResponseFromEhsasProgramService(address, httpClient, body, HttpMethod.Post);
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, response.Content.ReadAsStringAsync().Result.ToString(), EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-SubsidyPayResponse").Value);

            if (response.IsSuccessStatusCode)
            {
                SubsidyPaymentResponse subsidyPaymentResponse = JsonHelper.Deserialize<SubsidyPaymentResponse>(response.Content.ReadAsStringAsync().Result.ToString());
                if (subsidyPaymentResponse.info.response_desc == SUCCESS)
                {
                    await EhsasProgramHelper.AddSubsidyAsChargeAsync(request.RequestContext, request.CurrentTransactionId, subsidyPaymentResponse.subsidyPaymentResTxnInfo.totalSubsidy);       
                    return new VerifyEhsasProgramOtpResponse(new VerifyEhsasProgramOtpResponseEntity(subsidyPaymentResponse, request.AuthToken));
                }
                else
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                    {
                        LocalizedMessage = string.Format("{0} \n Code: {1}", subsidyPaymentResponse.info.response_desc, subsidyPaymentResponse.info.response_code),
                        LocalizedMessageParameters = new object[] { }
                    };
                }
            }
            else
            {
                SubsidyPaymentResponse subsidyPaymentResponse = JsonHelper.Deserialize<SubsidyPaymentResponse>(response.Content.ReadAsStringAsync().Result.ToString());
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0}", subsidyPaymentResponse?.info.response_desc ?? response.ReasonPhrase),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task GetEhsasProgramSubsidyPaymentInquiry(VerifyEhsasProgramOtpRequest request)
        {
            string address = EhsasProgramHelper.GetEhsasProgramConfigurations(request.RequestContext).GetProperty("CDCEHSASPROGRAMSUBSIDYPAYMENTINQUIRYLINK")?.ToString() ?? string.Empty;
            string body = JsonHelper.Serialize(PrepareSubsidyPaymentInquiryBody(request));
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, body, EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-PayInquiryRequest").Value);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AuthToken);

            HttpResponseMessage response = EhsasProgramHelper.GetResponseFromEhsasProgramService(address, httpClient, body, HttpMethod.Post);
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, response.Content.ReadAsStringAsync().Result.ToString(), EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-PayInquiryResponse").Value);
            SubsidyPaymentInquiryResponseEntity subsidyPaymentInquiryResponse = JsonHelper.Deserialize<SubsidyPaymentInquiryResponseEntity>(response.Content.ReadAsStringAsync().Result.ToString());
            
            if (subsidyPaymentInquiryResponse.info.response_desc != SUCCESS)
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0} \n Code: {1}", subsidyPaymentInquiryResponse.info.response_desc, subsidyPaymentInquiryResponse.info.response_code),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private SubsidyPaymentInquiryRequestEntity PrepareSubsidyPaymentInquiryBody(VerifyEhsasProgramOtpRequest request)
        {
            SubsidyPaymentInquiryRequestEntity subsidyPaymentInquiryRequestEntity = new SubsidyPaymentInquiryRequestEntity();

            subsidyPaymentInquiryRequestEntity.info = new Info
            {
                rrn = EhsasProgramHelper.GenerateRandomString(12),
                stan = EhsasProgramHelper.GenerateRandomString(6)
            };

            subsidyPaymentInquiryRequestEntity.paymentInquiryReqTxnInfo = new PaymentInquiryReqTxnInfo
            {
                authid = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.authId,
                cnic = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.cnic,
                merchantId = EhsasProgramHelper.GetMerchantId(request.RequestContext)?.GetProperty("CDCMERCHANTID")?.ToString() ?? string.Empty,
                dateTime = DateTime.Now.ToString("yyyyy-mm-dd")
            };
            return subsidyPaymentInquiryRequestEntity;
        }

        /// <summary>
        /// Call resend otp api
        /// </summary>
        /// <param name="resendOtpRequest"></param>
        /// <returns></returns>
        private async Task<ResendOtpResponse> GetResendOtpResponseAsync(ResendOtpRequest request)
        {
            string address = EhsasProgramHelper.GetEhsasProgramConfigurations(request.RequestContext).GetProperty("CDCEHSASPROGRAMRESENDOTPLINK")?.ToString() ?? string.Empty;
            string body = PrepareResendOtpRequestBody(request);
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, body, EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-ResendOTPRequest").Value);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AuthToken);
            HttpResponseMessage response = EhsasProgramHelper.GetResponseFromEhsasProgramService(address, httpClient, body, HttpMethod.Post);
            EhsasProgramResendOtpResponse ehsasProgramResendOtpResponse = JsonHelper.Deserialize<EhsasProgramResendOtpResponse>(response.Content.ReadAsStringAsync().Result.ToString());
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, body, EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-ResendOTPResponse").Value);

            if (response.IsSuccessStatusCode)
            {
                if (ehsasProgramResendOtpResponse.info.response_desc == SUCCESS)
                {
                    return new ResendOtpResponse(new ResendOtpResponseEntity(ehsasProgramResendOtpResponse, request.AuthToken));
                }
                else
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                    {
                        LocalizedMessage = string.Format("{0} \n Code: {1}", ehsasProgramResendOtpResponse.info.response_desc, ehsasProgramResendOtpResponse.info.response_code),
                        LocalizedMessageParameters = new object[] { }
                    };
                }
            }
            else
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0}", ehsasProgramResendOtpResponse?.info.response_desc ?? response.ReasonPhrase),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }

        private string PrepareResendOtpRequestBody(ResendOtpRequest request)
        {
            EhsasProgramResendOtpRequest otpRequest = new EhsasProgramResendOtpRequest();

            otpRequest.info = new Info
            {
                rrn = EhsasProgramHelper.GenerateRandomString(12),
                stan = EhsasProgramHelper.GenerateRandomString(6)
            };
            otpRequest.resendOtpReqTxnInfo = new ResendOtpReqTxnInfo
            {
                authId = request.AuthId,
                cnic = request.Cnic,
                merchantId = EhsasProgramHelper.GetMerchantId(request.RequestContext).GetProperty("CDCMERCHANTID")?.ToString() ?? string.Empty,
                tranDate = DateTime.Now.ToString("yyyy-MM-dd")

            };

            return JsonHelper.Serialize(otpRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SubsidyPaymentRequest PrepareOtpRequestBody(VerifyEhsasProgramOtpRequest request)
        {
            SubsidyPaymentRequest paymentRequest = new SubsidyPaymentRequest();

            paymentRequest.info = new Info
            {
                rrn = EhsasProgramHelper.GenerateRandomString(12),
                stan = EhsasProgramHelper.GenerateRandomString(6)
            };

            paymentRequest.subsidyPaymentReqTxnInfo = new SubsidyPaymentReqTxnInfo
            {
                authId = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.authId,
                cnic = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.cnic,
                dateTime = DateTime.Now.ToString("yyyyMMddhhmmss"),
                itemsCount = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.itemsCount,
                merchantId = EhsasProgramHelper.GetMerchantId(request.RequestContext).GetProperty("CDCMERCHANTID")?.ToString() ?? string.Empty,
                netAmount = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.netAmount,
                otp = request.OTP,
                totalSubsidy = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.totalSubsidy,
                totalValue = request.SubsidyInquiryResponse.subsidyInquiryResTxnInfo.totalValue
            };

            return paymentRequest;
        }
        
    }
}
