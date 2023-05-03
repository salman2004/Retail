using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.BackDateValidation.Model
{
    [DataContract]
    public class BackDateValidationRequest : Request
    {
        public BackDateValidationRequest(string deviceDateTime)
        {
            this.DeviceDateTime = deviceDateTime;
        }

        [DataMember]
        public string DeviceDateTime { get; set; }
    }
}
