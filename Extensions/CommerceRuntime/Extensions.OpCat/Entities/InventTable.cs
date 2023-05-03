using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.CustomOpCat.Entities
{
    public class InventTable : CommerceEntity
    {
        public InventTable() : base("InventTable")
        {
            //this.DEMO_MARGINCAP = demo_MarginCap;
            //this.DEMO_MARGINCAPPROTECTION = demo_MarginCapProtection;
        }

        [DataMember]
        [Column("CDCMARGINCAPPROTECTION")]
        public int CDCMARGINCAPPROTECTION { get; set; }
    }
}
