using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class SubsidyInquiryResponse
    {
        public List<SubsidyCommodityResTxnInfo> subsidyCommodityResTxnInfo { get; set; }
        public SubsidyInquiryResTxnInfo subsidyInquiryResTxnInfo { get; set; }
        public ResponseInfo info { get; set; }
    }

    public class SubsidyCommodityResTxnInfo
    {
        public string unit { get; set; }
        public decimal amount { get; set; }
        public decimal defaultRate { get; set; }
        public string code { get; set; }
        public decimal netAmount { get; set; }
        public decimal rate { get; set; }
        public decimal subsidy { get; set; }
        public decimal qty { get; set; }
        public string name { get; set; }
    }

    public class SubsidyInquiryResTxnInfo
    {
        public decimal totalValue { get; set; }
        public string dateTime { get; set; }
        public decimal totalSubsidy { get; set; }
        public decimal netAmount { get; set; }
        public string cnic { get; set; }
        public decimal itemsCount { get; set; }
        public string authId { get; set; }
    }

}
