using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Runtime.Serialization;
namespace CDC.Commerce.Runtime.FractionalSale.Entities
{
    class RetailConfigurationParameters : CommerceEntity
    {
        public RetailConfigurationParameters() : base("RetailConfigurationParameters")
        {

        }
        [Key]
        [DataMember]
        [Column("RECID")]
        public long RECID { get; set; }

        [DataMember]
        [Column("NAME")]
        public string NAME { get; set; }

        [DataMember]
        [Column("VALUE")]
        public string VALUE { get; set; }

        [DataMember]
        [Column("DATAAREAID")]
        public string DATAAREAID { get; set; }

        [DataMember]
        [Column("ROWVERSION")]
        public DateTime ROWVERSION { get; set; }
    }
}
