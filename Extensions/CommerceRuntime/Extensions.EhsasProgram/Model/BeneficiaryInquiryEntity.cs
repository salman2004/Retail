using System.Collections.Generic;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class BeneficiaryInquiryEntity
    {
        public List<Commodity> commodity { get; set; }
        public BeneVerificationResTxnInfo beneVerificationResTxnInfo { get; set; }
        public ResponseInfo info { get; set; }
    }

    public class BeneVerificationResTxnInfo
    {
        public string dateTime { get; set; }
        public string merchantId { get; set; }
        public string name { get; set; }
        public string cnic { get; set; }
        public decimal rateMargin { get; set; }
    }

    public class Commodity
    {
        public string unit { get; set; }
        public decimal defaultRate { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class ResponseInfo
    {
        public string response_code { get; set; }
        public string response_desc { get; set; }
        public string STAN { get; set; }
        public string RRN { get; set; }
    }
}
