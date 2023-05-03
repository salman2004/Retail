using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.HardwareStation.RFIDCardReader.Model
{
    public class CardReaderResponse
    {
        public string csdCardNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string writtenCardNumber { get; set; }
    }
}
