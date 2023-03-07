using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class SubsidyPaymentRequest
    {
        public Info info { get; set; }
        public SubsidyPaymentReqTxnInfo subsidyPaymentReqTxnInfo { get; set; }
    }
    
    public class SubsidyPaymentReqTxnInfo
    {
        public string authId { get; set; }
        public string cnic { get; set; }
        public string dateTime { get; set; }
        public decimal itemsCount { get; set; }
        public string merchantId { get; set; }
        public decimal netAmount { get; set; }
        public string otp { get; set; }
        public decimal totalSubsidy { get; set; }
        public decimal totalValue { get; set; }
    }
}
