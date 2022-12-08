using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.MarginCap.Entities
{
    public class GetMarginCapOnStoreAndLoyaltyProgram : CommerceEntity
    {
        public GetMarginCapOnStoreAndLoyaltyProgram() : base("GetMarginCapOnStoreAndLoyaltyProgram")
        {
        }

        [DataMember]
        [Column("ISMARGINCAPALLOWEDONSTOREANDLOYALTY")]
        public int ISMARGINCAPALLOWEDONSTOREANDLOYALTY { get; set; }

        [DataMember]
        [Column("MARGINCAPPERCENTAGE")]
        public string MARGINCAPPERCENTAGE { get; set; }

    }
}
