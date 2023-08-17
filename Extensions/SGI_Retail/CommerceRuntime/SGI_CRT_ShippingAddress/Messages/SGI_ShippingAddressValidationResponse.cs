namespace SGI.Commerce.Runtime.Extension.ShippingAddress.Messages
{
    using System.Runtime.Serialization;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using SGI.Commerce.Runtime.Extension.ShippingAddress.DataModel;

    [DataContract]
    public sealed class SGI_ShippingAddressValidationResponse : Response
    {
        ///<summary>
        ///Initialize a new instance of the <see cref="SGI_ShippingAddressValidationResponse"/> class 
        ///</summary>
        public SGI_ShippingAddressValidationResponse(SGI_Address result)
        {
            this.Result = result;
        }

        [DataMember]
        public SGI_Address Result { get; private set; }
    }
}
