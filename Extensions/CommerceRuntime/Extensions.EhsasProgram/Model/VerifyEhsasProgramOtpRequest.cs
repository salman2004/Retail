using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    [DataContract]
    public class VerifyEhsasProgramOtpRequest : Request
    {
        public VerifyEhsasProgramOtpRequest(SubsidyInquiryResponse subsidyInquiryResponse, string otp, string authToken, string currentTransactionId)
        {
            this.SubsidyInquiryResponse = subsidyInquiryResponse;
            this.OTP = otp;
            this.AuthToken = authToken;
            this.CurrentTransactionId = currentTransactionId;
        }

        [DataMember]
        public string OTP{ get; set; }

        [DataMember]
        public SubsidyInquiryResponse SubsidyInquiryResponse { get; set; }

        [DataMember]
        public string AuthToken{ get; set; }

        [DataMember]
        public string CurrentTransactionId{ get; set; }

    }
}
