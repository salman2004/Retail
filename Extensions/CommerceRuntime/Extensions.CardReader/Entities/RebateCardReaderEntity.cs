using CDC.Commerce.Runtime.CardReader.Entities;
using System;


namespace CDC.Commerce.Runtime.CardReader.Entities
{ 
    public class RebateCardReaderEntity : CardReaderEntity
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string writtenCardNumber { get; set; }
        public string rank { get; set; }
        public string limit { get; set; }
        public string balance { get; set; }

        public DateTime lastTransactionDateTime { get; set; }
        public bool isCardActivated { get; set; }
        public bool isCardBlocked { get; set; }
    }
}
