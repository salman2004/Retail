using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.HardwareStation.RFIDCardReader.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class TransactionalData
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string csdCardNumber { get; set; }

        [DataMember]
        public string usedBalance { get; set; }

        [DataMember]
        public string cardInfo { get; set; }

        [DataMember]
        public bool isCardRebate { get; set; }

        public int MyProperty { get; set; }
    }
}
