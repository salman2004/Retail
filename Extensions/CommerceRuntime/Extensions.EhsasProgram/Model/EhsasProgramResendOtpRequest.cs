using CDC.Commerce.Runtime.EhsasProgram.Model;

namespace CDC.Commerce.Runtime.EhsasProgram.ResendOtpMode
{
    public class EhsasProgramResendOtpRequest
    {
        public Info info { get; set; }
        public ResendOtpReqTxnInfo resendOtpReqTxnInfo { get; set; }
    }

    public class ResendOtpReqTxnInfo
    {
        public string authId { get; set; }
        public string cnic { get; set; }
        public string merchantId { get; set; }
        public string tranDate { get; set; }
    }

}
