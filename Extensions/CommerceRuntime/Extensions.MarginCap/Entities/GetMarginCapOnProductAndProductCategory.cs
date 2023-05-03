using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.MarginCap.Entities
{
    public class GetMarginCapOnProductAndProductCategory : CommerceEntity
    {
        public GetMarginCapOnProductAndProductCategory() : base("GetMarginCapOnProductAndProductCategory")
        {
        }

        [DataMember]
        [Column("ISMARGINCAPALLOWED")]
        public int ISMARGINCAPALLOWED { get; set; }

        [DataMember]
        [Column("MARGINCAPPERCENTAGE")]
        public string MARGINCAPPERCENTAGE { get; set; }

        [DataMember]
        [Column("EXCLUDEDISCOUNT")]
        public int EXCLUDEDISCOUNT { get; set; }
    }
}