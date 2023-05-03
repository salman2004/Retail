using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class BeneficiaryInquiryRequestBody
    {
        public BeneVerificationReqTxnInfo beneVerificationReqTxnInfo { get; set; }
        public Info info { get; set; }
    }

    public class BeneVerificationReqTxnInfo
    {
        public string cnic { get; set; }
        public string dateTime { get; set; }
        public string merchantId { get; set; }
    }

    public class Info
    {
        public string rrn { get; set; }
        public string stan { get; set; }
    }
}
