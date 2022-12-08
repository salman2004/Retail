using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.MarginCap.Entities
{
    public class RetailAffiliation : CommerceEntity
    {
        public RetailAffiliation() : base("RetailAffiliation")
        {
            //this.DEMO_MARGINCAP = demo_MarginCap;
            //this.DEMO_MARGINCAPPROTECTION = demo_MarginCapProtection;
        }

        [DataMember]
        [Column("CDCMARGINCAPPROTECTION")]
        public int CDCMARGINCAPPROTECTION { get; set; }
    }
}
