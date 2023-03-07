using CDC.Commerce.Runtime.EhsasProgram.Model;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.EhsasProgram
{
    [DataContract]
    public class ResendOtpRequest : Request
    {
        public ResendOtpRequest(string authToken, string authId, string cnic, string merhcantId, string currentTransactionId)
        {
            this.AuthToken = authToken;
            this.AuthId = authId;
            this.Cnic = cnic;
            this.MerchantId = merhcantId;
            this.CurrentTransactionId = currentTransactionId;
        }

        [DataMember]
        public string AuthToken { get; set; }

        [DataMember]
        public string AuthId { get; set; }

        [DataMember]
        public string Cnic { get; set; }

        [DataMember]
        public string MerchantId { get; set; }

        [DataMember]
        public string CurrentTransactionId { get; set; }

    }
}
