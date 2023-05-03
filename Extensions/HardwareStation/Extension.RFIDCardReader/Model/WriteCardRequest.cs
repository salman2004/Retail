using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.HardwareStation.RFIDCardReader.Model
{
    [DataContract]
    public class WriteCardRequest
    {
        [DataMember]
        public string shopCode { get; set; }
           
        [DataMember]
        public string usedPoints { get; set; }

        [DataMember]
        public string cardInfo { get; set; }

        [DataMember]
        public string csdCardNumber { get; set; }

        [DataMember]
        public  string writtenCardNumebr { get; set; }

        [DataMember]
        public bool isCardRebate { get; set; }
    }
}
