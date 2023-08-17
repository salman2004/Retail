namespace SGI.Commerce.Runtime.Extension.ShippingAddress.Messages
{
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class SGI_ShippingAddressValidationRequest : Request
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SGI_ShippingAddressValidationRequest"/>  class.
        /// </summary>
        /// <param name="zipcode"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="country"></param>
        public SGI_ShippingAddressValidationRequest(string zipcode, string city, string state, string country)
        {
            this.Zipcode = zipcode;
            this.City = city;
            this.State = state;
            this.Country = country;
            
        }

        ///<summary>
        /// Get the zipcode related data to the rquest
        ///</summary>
        [DataMember]
        public string Zipcode { get; private set; }

        ///<summary>
        /// Get the city related data to the rquest
        ///</summary>
        [DataMember]
        public string City { get; private set; }

        ///<summary>
        /// Get the State related data to the rquest
        ///</summary>
        [DataMember]
        public string State { get; private set; }

        ///<summary>
        /// Get the Country related data to the rquest
        ///</summary>
        [DataMember]
        public string Country { get; private set; }
    }
}
