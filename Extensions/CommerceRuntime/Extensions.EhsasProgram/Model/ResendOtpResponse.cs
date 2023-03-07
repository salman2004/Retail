using CDC.Commerce.Runtime.EhsasProgram.ResendOtpMode;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram
{
    [DataContract]
    public class ResendOtpResponse : Response
    {
        public ResendOtpResponse(ResendOtpResponseEntity ResendOtpResponseEntity)
        {
            this.ResendOtpResponseEntity = ResendOtpResponseEntity;
        }

        [DataMember]
        public ResendOtpResponseEntity ResendOtpResponseEntity { get; set; }
    }
}
