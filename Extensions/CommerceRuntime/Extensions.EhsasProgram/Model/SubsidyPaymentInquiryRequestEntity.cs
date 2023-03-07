using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class SubsidyPaymentInquiryRequestEntity
    {
        public Info info { get; set; }
        public PaymentInquiryReqTxnInfo paymentInquiryReqTxnInfo { get; set; }
    }

    public class PaymentInquiryReqTxnInfo
    {
        public string authid { get; set; }
        public string cnic { get; set; }
        public string dateTime { get; set; }
        public string merchantId { get; set; }
    }
}
