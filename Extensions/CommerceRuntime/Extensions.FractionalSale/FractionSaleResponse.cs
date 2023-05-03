using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.FractionalSale
{
    [DataContract]
    public sealed class FractionSaleResponse : Response
    {
        public FractionSaleResponse(bool status, string message)
        {
            Status = status;
            Message = message;
        }
        [Key]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public bool Status { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}
