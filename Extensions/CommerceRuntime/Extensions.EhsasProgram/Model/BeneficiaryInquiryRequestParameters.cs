using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    [DataContract]
    public class BeneficiaryInquiryRequestParameters
    {
        [DataMember]
        public string CNICNumber { get; set; }

        [DataMember]
        public Product [] Products{ get; set; }

        [DataMember]
        public string currentTransactionId { get; set; }
    }

    [DataContract]
    public class Product
    {
        [DataMember]
        public string ItemId { get; set; }

        [DataMember]
        public string InventDimId{ get; set; }

        [DataMember]
        public decimal Amount{ get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public Int64 ProductId { get; set; }

    }


}
