using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;
using CDC.Commerce.Runtime.EhsasProgram.Model;

namespace CDC.Commerce.Runtime.EhsasProgram
{
    [DataContract]
    public class EhsasProgramResponse : Response
    {
        public EhsasProgramResponse(EhsasProgramEntity ehsasProgram)
        {
            this.EhsasProgram = ehsasProgram;
        }

        [DataMember]
        public EhsasProgramEntity EhsasProgram { get; private set; }

        

    }
}
