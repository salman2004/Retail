using System;


namespace CDC.Commerce.HardwareStation.RFIDCardReader.Model
{
    public class RebateCardReaderResponse : CardReaderResponse
    {
        public RebateCardReaderResponse()
        {

        }        
        public string rank { get; set; }
        public string limit { get; set; }
        public string balance { get; set; }

        public DateTime lastTransactionDateTime { get; set; }
        public bool isCardActivated { get; set; }
        public bool isCardBlocked { get; set; }
    }
}
