using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.MarginCap.Entities
{
    public class EcoResCategory : CommerceEntity
    {
        public EcoResCategory() : base("EcoResCategory")
        {
            //this.DEMO_MARGINCAP = demo_MarginCap;
            //this.DEMO_MARGINCAPPROTECTION = demo_MarginCapProtection;
        }

        [DataMember]
        [Column("CDCMARGINCAPPROTECTION")]
        public int CDCMARGINCAPPROTECTION { get; set; }
    }
}
