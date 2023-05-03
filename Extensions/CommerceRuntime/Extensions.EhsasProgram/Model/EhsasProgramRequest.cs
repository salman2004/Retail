using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;
using CDC.Commerce.Runtime.EhsasProgram.Model;

namespace CDC.Commerce.Runtime.EhsasProgram
{
    [DataContract]
    public class EhsasProgramRequest : Request
    {
        public EhsasProgramRequest(string cnicNumber, Product [] products,string CurrentTransactionId)
        {
            this.CNICNumber = cnicNumber;
            this.Products = products;
            this.CurrentTransactionId = CurrentTransactionId;
        }

        [DataMember]
        public string CNICNumber { get; set; }

        [DataMember]
        public Product [] Products{ get; set; }

        [DataMember]
        public string CurrentTransactionId { get; set; }
    }
}
