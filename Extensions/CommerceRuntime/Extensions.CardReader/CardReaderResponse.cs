using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.CardReader
{
    [DataContract]
    public class CardReaderResponse : Response
    {
        public CardReaderResponse(bool isCardActivated)
        {
            this.IsCardActivated = isCardActivated;
        }

        [DataMember]
        public bool IsCardActivated { get; private set; }
    }
}
