using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CDC.Commerce.Runtime.FBRIntegration.Entities
{
    public class InvoiceId : CommerceEntity
    {
        public InvoiceId() : base("InvoiceId")
        {

        }

        [DataMember]
        [Column("CDCINVOICEID")]
        public string InvoiceNumber
        {
            get { return (string)this[InvoiceNumber]; }
            set { this[InvoiceNumber] = value; }
        }
    }
}
