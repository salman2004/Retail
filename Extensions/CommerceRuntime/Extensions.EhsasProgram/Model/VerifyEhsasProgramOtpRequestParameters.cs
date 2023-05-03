using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    [DataContract]
    public class VerifyEhsasProgramOtpRequestParameters
    {
        [DataMember]
        public string OTP { get; set; }

        [DataMember]
        public SubsidyInquiryResponse SubsidyInquiryResponse { get; set; }

        [DataMember]
        public string AuthToken { get; set; }

        [DataMember]
        public string currentTransactionId { get; set; }
    }
}
