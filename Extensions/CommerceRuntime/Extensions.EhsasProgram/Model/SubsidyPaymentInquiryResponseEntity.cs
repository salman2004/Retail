using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class SubsidyPaymentInquiryResponseEntity
    {
        public PaymentInquiryResTxnInfo paymentInquiryResTxnInfo { get; set; }
        public ResponseInfo info { get; set; }
    }


    public class PaymentInquiryResTxnInfo
    {
        public decimal totalAmount { get; set; }
        public decimal totalSubsidy { get; set; }
        public int merchantId { get; set; }
        public decimal netAmount { get; set; }
        public string transactionDateTime { get; set; }
        public string transactionStatus { get; set; }
        public string stan { get; set; }
        public int id { get; set; }
        public string billNumber { get; set; }
        public string paymentStatus { get; set; }
        public string rrn { get; set; }
        public int beneficiaryId { get; set; }
    }
}
