using System;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.BackDateValidation.Model
{
    [DataContract]
    public class BackDateValidationResponse : Response
    {
        public BackDateValidationResponse(bool isDateValidated)
        {
            this.IsDateValidated = isDateValidated;
        }

        [DataMember]
        public bool IsDateValidated { get; set; }
    }
}
