using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;


namespace CDC.Commerce.Runtime.EhsasProgram.ResendOtpMode
{
    [DataContract]
    public class ResendOtpResponseEntity : CommerceEntity
    {
        public ResendOtpResponseEntity(EhsasProgramResendOtpResponse ehsasProgramResendOtpResponse, string AuthToken) : base("ResendOtpResponseEntity")
        {
            this.EhsasProgramResendOtpResponse = ehsasProgramResendOtpResponse;
            this.AuthToken = AuthToken;
        }

        [DataMember]
        [Column("EhsasProgramResendOtpResponse")]
        public EhsasProgramResendOtpResponse EhsasProgramResendOtpResponse { get; set; }

        [SystemDataAnnotations.Key]
        [Key]
        [DataMember]
        [Column("AuthToken")]
        public string AuthToken { get; set; }
    }
}
