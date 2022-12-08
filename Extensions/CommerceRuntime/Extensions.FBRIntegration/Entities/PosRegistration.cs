using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CDC.Commerce.Runtime.FBRIntegration.Entities
{
    public class PosRegistration : CommerceEntity
    {
        public PosRegistration() : base("PosRegistration")
        {

        }

        [DataMember]
        [Column("CDCFBRPOSREGISTRATIONID")]
        public string CDCFBRPOSREGISTRATIONID
        {
            get { return (string)this[CDCFBRPOSREGISTRATIONID]; }
            set { this[CDCFBRPOSREGISTRATIONID] = value; }
        }
    }
}
