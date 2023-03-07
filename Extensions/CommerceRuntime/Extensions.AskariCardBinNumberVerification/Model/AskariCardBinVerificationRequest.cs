using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.AskariCardBinNumberVerification.Model
{
    [DataContract]
    public class AskariCardBinVerificationRequest : Request
    {
        public AskariCardBinVerificationRequest(string cardNumber, string transactionId)
        {
            this.CardNumber = cardNumber;
            this.TransactionId = transactionId;
        }
        
        [DataMember]
        public string CardNumber { get; set; }

        [DataMember]
        public string TransactionId { get; set; }

    }
}
