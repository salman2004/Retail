using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.HardwareStation.RFIDCardReader.Model
{
    public class LoyaltyCardReaderResponse : CardReaderResponse
    {
        public LoyaltyCardReaderResponse() 
        {

        }
        
        
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
