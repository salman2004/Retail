using CDC.Commerce.Runtime.EhsasProgram.Model;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.EhsasProgram.ResendOtpMode
{
    [DataContract]
    public class ResendOtpRequestParameter
    {
        [DataMember]
        public string AuthToken { get; set; }

        [DataMember]
        public string AuthId { get; set; }

        [DataMember]
        public string Cnic { get; set; }
        
        [DataMember]
        public string MerchantId { get; set; }

        [DataMember]
        public string currentTransactionId { get; set; }

    }
}
