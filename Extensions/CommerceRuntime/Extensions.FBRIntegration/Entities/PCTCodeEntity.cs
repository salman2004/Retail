using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.FBRIntegration.Entities
{
    class PCTCodeEntity : CommerceEntity
    {   
        public PCTCodeEntity() : base("PCTCode")
        {
        }

        [DataMember]
        [Column("FBRHSCODE")]
        public int PCTCode { get; set; }
        

    }
}
