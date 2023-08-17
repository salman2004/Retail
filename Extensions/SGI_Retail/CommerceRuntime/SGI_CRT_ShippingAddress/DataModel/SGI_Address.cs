namespace SGI.Commerce.Runtime.Extension.ShippingAddress.DataModel
{
    using System.Runtime.Serialization;
    using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;

    public class SGI_Address : CommerceEntity
    {
        private const string Zipcolumn = "Zipcode";
        private const string Citycolumn = "City";
        private const string Statecolumn = "State";
        private const string Idcolumn = "RecId";

        public SGI_Address() : base(nameof(SGI_Address))
        { }

        [DataMember]
        [Column(Zipcolumn)]
        public string Zipcode
        {
            get { return (string)this[Zipcolumn]; }
            set { this[Zipcolumn] = value; }
        }

        [DataMember]
        [Column(Citycolumn)]
        public string City
        {
            get { return (string)this[Citycolumn]; }
            set { this[Citycolumn] = value; }
        }

        [DataMember]
        [Column(Statecolumn)]
        public string State
        {
            get { return (string)this[Statecolumn]; }
            set { this[Statecolumn] = value; }
        }
        [Key]
        [DataMember]
        [Column(Idcolumn)]
        public long Id
        {
            get { return (long)this[Idcolumn]; }
            set { this[Idcolumn] = value; }
        }

    }
}
