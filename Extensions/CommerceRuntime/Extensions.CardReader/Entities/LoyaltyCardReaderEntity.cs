using CDC.Commerce.Runtime.CardReader.Entities;
using System;

namespace CDC.Commerce.Runtime.CardReader.Entities
{
    public class LoyaltyCardReaderEntity : CardReaderEntity
    {
        
        public string firstName { get; set; }
        public string lastName { get; set; }

        public string writtenCardNumber { get; set; }
        public string cateogry { get; set; }

        public DateTime lastTransactionDateTime { get; set; }
        public bool isCardActivated { get; set; }
        public bool isCardBlocked { get; set; }
        public string lastShopCode { get; set; }
        public string totalPoints { get; set; }
        public string balancePoints { get; set; }
        public string usedPoints { get; set; }

    }
}
