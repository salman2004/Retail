using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

namespace CDC.CommerceRuntime.Entities.DataModel
{
    
    public class RetailParameters : CommerceEntity
    {
        
        public RetailParameters():base("RetailParameters")
        {            
            //this.DEMO_MARGINCAP = demo_MarginCap;
            //this.DEMO_MARGINCAPPROTECTION = demo_MarginCapProtection;
        }
        [DataMember]
        [Column("CDCMARGINCAPPROTECTION")]
        public int CDCMARGINCAPPROTECTION { get; set; }
        [DataMember]
        [Column("CDCMARGINCAP")]
        public string CDCMARGINCAP { get; set; }
    }
}
