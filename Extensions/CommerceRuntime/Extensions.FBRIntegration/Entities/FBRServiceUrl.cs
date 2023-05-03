using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CDC.Commerce.Runtime.FBRIntegration.Entities
{
    public class FBRServiceUrl : CommerceEntity
    {
        public FBRServiceUrl() : base("FBRServiceUrl")
        {
        }

        [DataMember]
        [Column("CDCFBRSERVERURL")]
        public string CDCFBRSERVERURL
        {
            get { return (string)this[CDCFBRSERVERURL]; }
            set { this[CDCFBRSERVERURL] = value; }
        }

        [DataMember]
        [Column("CDCFBRSERVICECHECKURL")]
        public string CDCFBRSERVICECHECKURL
        {
            get { return (string)this[CDCFBRSERVICECHECKURL]; }
            set { this[CDCFBRSERVICECHECKURL] = value; }
        }
    }
}
