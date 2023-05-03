

namespace CDC.RetailServer.EhsasProgram
{
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
    using System.Threading.Tasks;
    using CDC.Commerce.Runtime.EhsasProgram;
    using CDC.Commerce.Runtime.EhsasProgram.Model;
    using CDC.Commerce.Runtime.EhsasProgram.ResendOtpMode;

    [RoutePrefix("EhsasProgram")]
    [BindEntity(typeof(EhsasProgramEntity))]
    public class EhsasProgramController : IController
    {
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public virtual async Task<EhsasProgramEntity> GetEhsasProgramVerification(IEndpointContext context, BeneficiaryInquiryRequestParameters beneficiaryParameters)
        {   
            var request = new EhsasProgramRequest(beneficiaryParameters.CNICNumber, beneficiaryParameters.Products, beneficiaryParameters.currentTransactionId);
            EhsasProgramResponse response = await context.ExecuteAsync<EhsasProgramResponse>(request);
            return response.EhsasProgram; 
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public virtual async Task<VerifyEhsasProgramOtpResponseEntity> VerifyEhsasProgramOtp(IEndpointContext context, VerifyEhsasProgramOtpRequestParameters otpRequestParameters)
        {
            var request = new VerifyEhsasProgramOtpRequest(otpRequestParameters.SubsidyInquiryResponse, otpRequestParameters.OTP, otpRequestParameters.AuthToken, otpRequestParameters.currentTransactionId);
            VerifyEhsasProgramOtpResponse response = await context.ExecuteAsync<VerifyEhsasProgramOtpResponse>(request);
            return response.VerifyEhsasProgramOtpResponseEntity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public virtual async Task<ResendOtpResponseEntity> ResendEhsasProgramOtp(IEndpointContext context, ResendOtpRequestParameter resendOtpRequestParameters)
        {
            var request = new ResendOtpRequest(resendOtpRequestParameters.AuthToken,resendOtpRequestParameters.AuthId, resendOtpRequestParameters.Cnic, resendOtpRequestParameters.MerchantId, resendOtpRequestParameters.currentTransactionId);
            ResendOtpResponse response = await context.ExecuteAsync<ResendOtpResponse>(request);
            return response.ResendOtpResponseEntity;
        }
    }
}
