namespace CDC
{
    namespace Commerce.HardwareStation.RFIDCardReader
    {
        using System.Runtime.Serialization;

        /// <summary>
        /// Ping request class.
        /// </summary>
        [DataContract]
        public class CustomRequest
        {
            /// <summary>
            /// Gets or sets the message string.
            /// </summary>
            [DataMember]
            public string Message { get; set; }
        }
    }
}