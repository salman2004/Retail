using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CDC.Commerce.Runtime.EhsasProgram.Model;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Framework.Serialization;
using Microsoft.Dynamics.Commerce.Runtime.Messages;


namespace CDC.Commerce.Runtime.EhsasProgram
{
    class EhsasProgramRequestHandler : IRequestHandlerAsync
    {

        private const string EHSASPROGRAM = "EhsasProgram";
        private const string SUCCESS = "Success";

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(EhsasProgramRequest)
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
            if (reqType == typeof(EhsasProgramRequest))
            {
                return await GetEhsasProgramResponse((EhsasProgramRequest)request);
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
        /// <param name="ehsasProgramRequest"></param>
        /// <returns></returns>
        private  async Task<EhsasProgramResponse> GetEhsasProgramResponse(EhsasProgramRequest request)
        {
            //await EhsasProgramHelper.GetCurrentSalesTransactionAsync(request.CurrentTransactionId, request.RequestContext);
            string authToken = await GetAcessTokenForChannelService(request.RequestContext, request.CurrentTransactionId);
            EhsasProgramHelper.GetEhsasProgramCodes(request.RequestContext, request.Products, out List<ExtensionsEntity> entities);
            if (entities.Where(en => string.IsNullOrEmpty(en.GetProperty("CDCEHSASPROGRAMCODE")?.ToString() ?? null)).Count() == request.Products.Length)
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0}", "There are no ehsas program applicable products"),
                    LocalizedMessageParameters = new object[] { }
                };
            }

            SubsidyInquiryResponse response = await GetEhsasProgramDiscountsAsync(request, entities, authToken);
            EhsasProgramEntity entity = new EhsasProgramEntity(true, response, authToken);
            return new EhsasProgramResponse(entity);
        }

        /// <summary>
        /// Get ehsas program discounts
        /// </summary>
        private async Task<SubsidyInquiryResponse> GetEhsasProgramDiscountsAsync(EhsasProgramRequest request, List<ExtensionsEntity> entities, string authToken)
        {
            //response = new SubsidyInquiryResponse();
            //authToken = await GetAcessTokenForChannelService(request.RequestContext, request.CurrentTransactionId);
            List<Commodity> commodities = await GetBeneficiaryInquiryAsync(authToken, request.CNICNumber, request);

            if (entities.Where(en => commodities.Any(cm => cm.code == (en.GetProperty("CDCEHSASPROGRAMCODE")?.ToString() ?? string.Empty))).Count() > 0)
            {
                return await GetSubsidyInquiryAsync(authToken, commodities, entities, request);
            }
            else
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0}", "There are no ehsas program applicable products"),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }
        
        /// <summary>
        /// Response from Subsidy Inquiry
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name=""></param>
        private async Task<SubsidyInquiryResponse> GetSubsidyInquiryAsync(string authToken, List<Commodity> commodities, List<ExtensionsEntity> entities, EhsasProgramRequest request)
        {
            string body = JsonHelper.Serialize(EhsasProgramHelper.GetSubsidyInquiryParamters(request.RequestContext,commodities, entities, request.Products, request.CNICNumber));
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, body, EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-GenerateOTPRequest").Value);

            string url = EhsasProgramHelper.GetEhsasProgramConfigurations(request.RequestContext).GetProperty("CDCEHSASPROGRAMSUBSIDYINQUIRYLINK")?.ToString() ?? string.Empty; ;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            HttpResponseMessage response = EhsasProgramHelper.GetResponseFromEhsasProgramService(url, httpClient, body, HttpMethod.Post);
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, response.Content.ReadAsStringAsync().Result.ToString(), EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-GenerateOTPResponse").Value);
            SubsidyInquiryResponse subsidyInquiryResponse = JsonHelper.Deserialize<SubsidyInquiryResponse>(response.Content.ReadAsStringAsync().Result.ToString());
            if (response.IsSuccessStatusCode)
            {                
                if (subsidyInquiryResponse.info.response_desc == SUCCESS)
                {
                    return subsidyInquiryResponse;
                }
                else
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                    {
                        LocalizedMessage = string.Format("{0} \n Code: {1}", subsidyInquiryResponse.info.response_desc, subsidyInquiryResponse.info.response_code),
                        LocalizedMessageParameters = new object[] { }
                    };
                }
            }
            else
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0}", subsidyInquiryResponse.info.response_desc),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }
       
        private async Task<List<Commodity>> GetBeneficiaryInquiryAsync( string authToken ,string cnicNumber, EhsasProgramRequest request)
        {
            BeneficiaryInquiryRequestBody beneficiaryInquiryRequestBody = new BeneficiaryInquiryRequestBody();
            BeneVerificationReqTxnInfo beneVerificationReqTxn = new BeneVerificationReqTxnInfo();
            Info info = new Info();
            info.rrn = EhsasProgramHelper.GenerateRandomString(12);
            info.stan = EhsasProgramHelper.GenerateRandomString(6);
            beneVerificationReqTxn.cnic = cnicNumber;
            beneVerificationReqTxn.dateTime = DateTime.Now.ToString("yyyyMMddhhmmss");
            beneVerificationReqTxn.merchantId = EhsasProgramHelper.GetMerchantId(request.RequestContext)?.GetProperty("CDCMERCHANTID")?.ToString() ?? string.Empty;
            beneficiaryInquiryRequestBody.info = info;
            beneficiaryInquiryRequestBody.beneVerificationReqTxnInfo = beneVerificationReqTxn;
            string body = JsonHelper.Serialize(beneficiaryInquiryRequestBody);
            string url = EhsasProgramHelper.GetEhsasProgramConfigurations(request.RequestContext)?.GetProperty("CDCEHSASPROGRAMBENEVERIFICATIONLINK")?.ToString() ?? string.Empty;

            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, body, EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-BeneficiaryRequest").Value);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            HttpResponseMessage response = EhsasProgramHelper.GetResponseFromEhsasProgramService(url, httpClient, body, HttpMethod.Post);
            await EhsasProgramHelper.SaveInfoCodeLine(request.CurrentTransactionId, request.RequestContext, response.Content.ReadAsStringAsync().Result.ToString(), EhsasProgramHelper.GetConfigurationParameters(request.RequestContext, "EhsaasPInfoCode-BeneficiaryResponse").Value);
            BeneficiaryInquiryEntity beneficiaryInquiryEntity = JsonHelper.Deserialize<BeneficiaryInquiryEntity>(response.Content.ReadAsStringAsync().Result.ToString());
            
            if (response.IsSuccessStatusCode)
            {
                if (beneficiaryInquiryEntity.info.response_desc == SUCCESS)
                {
                    return beneficiaryInquiryEntity.commodity;
                }
                else
                {
                    throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                    {
                        LocalizedMessage = string.Format("{0} \n Code: {1}", beneficiaryInquiryEntity.info.response_desc, beneficiaryInquiryEntity.info.response_code),
                        LocalizedMessageParameters = new object[] { }
                    };
                }                
            }
            else
            {
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Ehsas Program")
                {
                    LocalizedMessage = string.Format("{0}", beneficiaryInquiryEntity.info.response_desc),
                    LocalizedMessageParameters = new object[] { }
                };
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAcessTokenForChannelService(RequestContext context, string transactionid)
        {
            ExtensionsEntity entity = EhsasProgramHelper.GetEhsasProgramConfigurations(context);
            string url = entity.GetProperty("CDCEHSASPROGRAMAUTHENTICATIONLINK")?.ToString() ?? string.Empty ;
            string body = EhsasProgramHelper.PrepareAuthenticationRequestParameters(entity);
            await EhsasProgramHelper.SaveInfoCodeLine(transactionid, context, body, EhsasProgramHelper.GetConfigurationParameters(context, "EhsaasPInfoCode-AuthenticationRequest").Value);

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = EhsasProgramHelper.GetResponseFromEhsasProgramService(url, httpClient, body, HttpMethod.Post);
            await EhsasProgramHelper.SaveInfoCodeLine(transactionid, context, response.Content.ReadAsStringAsync().Result.ToString(), EhsasProgramHelper.GetConfigurationParameters(context, "EhsaasPInfoCode-AuthenticationResponse").Value);
            AuthenticationEntity authenticationEntity = JsonHelper.Deserialize<AuthenticationEntity>(response.Content.ReadAsStringAsync().Result.ToString());
            return authenticationEntity.token;
        }

        
    }
}
