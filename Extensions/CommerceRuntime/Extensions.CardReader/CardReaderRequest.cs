using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.CardReader
{
    [DataContract]
    public class CardReaderRequest : Request
    {
        public CardReaderRequest(string cardNumber, string cnicNumber)
        {
            this.CardNumber = cardNumber;
            this.CNICNumber = cnicNumber;
        }

        [DataMember]
        public string CardNumber { get; set; }
        [DataMember]
        public string CNICNumber { get; set; }
    }
}
