using System.Runtime.Serialization;


namespace CDC.Commerce.HardwareStation.RFIDCardReader
{
    [DataContract]
    public class ActivateCardRequest
    {
        /// <summary>
        /// Gets or sets the message string.
        /// </summary>
        [DataMember]
        public string cardInfo { get; set; }

        /// <summary>
        /// Gets or sets the message string.
        /// </summary>
        [DataMember]
        public string csdCardNumber { get; set; }
        /// <summary>
        /// Gets or sets the message string.
        /// </summary>
        [DataMember]
        public string writtenCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the message string.
        /// </summary>
        [DataMember]
        public bool isCardActivated { get; set; }

    }
}
