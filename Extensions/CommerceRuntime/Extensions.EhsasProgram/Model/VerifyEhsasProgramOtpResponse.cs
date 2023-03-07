using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    [DataContract]
    public class VerifyEhsasProgramOtpResponse : Response
    {
        public VerifyEhsasProgramOtpResponse(VerifyEhsasProgramOtpResponseEntity verifyEhsasProgramOtpResponseEntity)
        {
            this.VerifyEhsasProgramOtpResponseEntity = verifyEhsasProgramOtpResponseEntity;
        }

        [DataMember]
        public VerifyEhsasProgramOtpResponseEntity VerifyEhsasProgramOtpResponseEntity { get; set; }
        
    }
}
