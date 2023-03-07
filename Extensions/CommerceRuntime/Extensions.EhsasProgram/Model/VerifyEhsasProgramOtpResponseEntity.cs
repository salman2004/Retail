using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    [DataContract]
    public class VerifyEhsasProgramOtpResponseEntity : CommerceEntity
    {
        public VerifyEhsasProgramOtpResponseEntity(SubsidyPaymentResponse subsidyPaymentResponse, string authToken) :  base("VerifyEhsasProgramOtpResponseEntity")
        {
            this.SubsidyPaymentResponse = subsidyPaymentResponse;
            this.AuthToken = authToken;
        }

        [DataMember]
        [Column("SubsidyPaymentResponse")]
        public SubsidyPaymentResponse SubsidyPaymentResponse { get; set; }

        [SystemDataAnnotations.Key]
        [Key]
        [DataMember]
        [Column("AuthToken")]
        public string AuthToken { get; set; }
        
    }
}
