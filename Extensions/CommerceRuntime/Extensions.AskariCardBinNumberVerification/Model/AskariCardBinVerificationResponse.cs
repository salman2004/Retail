using System;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.AskariCardBinNumberVerification.Model
{
    [DataContract]
    public class AskariCardBinVerificationResponse : Response
    {
        public AskariCardBinVerificationResponse(bool isDateValidated)
        {
            this.IsDateValidated = isDateValidated;
        }

        [DataMember]
        public bool IsDateValidated { get; set; }
    }
}
