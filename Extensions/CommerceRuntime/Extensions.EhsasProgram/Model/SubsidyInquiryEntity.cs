using System.Collections.Generic;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class SubsidyInquiryEntity
    {
        public List<SubsidyInquiryCommodity> commodity { get; set; }
        public Info info { get; set; }
        public SubsidyInquiryReqTxnInfo subsidyInquiryReqTxnInfo { get; set; }
    }

    //[DataContract(Name = "Commodity")]
    public class SubsidyInquiryCommodity
    {
        public string code { get; set; }
        public string name { get; set; }
        public decimal defaultRate { get; set; }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal amount { get; set; }
    }

    public class SubsidyInquiryReqTxnInfo
    {
        public string cnic { get; set; }
        public string dateTime { get; set; }
        public decimal itemsCount { get; set; }
        public string merchantId { get; set; }
        public decimal totalValue { get; set; }
    }

}
