using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.FractionalSale.Entities
{
    class CDCProductFractionalSale : CommerceEntity
    {
        public CDCProductFractionalSale() : base("CDCProductFractionalSale")
        {
        }

        [Key]
        [DataMember]
        [Column("RECID")]
        public long RECID { get; set; }

        [DataMember]
        [Column("CATEGORY")]
        public long CATEGORY { get; set; }

        [DataMember]
        [Column("PRODUCT")]
        public long PRODUCT { get; set; }

        [DataMember]
        [Column("VARIANT")]
        public long VARIANT { get; set; }

        [DataMember]
        [Column("CATEGORYHIERARCHY")]
        public long CATEGORYHIERARCHY { get; set; }

        [DataMember]
        [Column("STORENUMBER")]
        public string STORENUMBER { get; set; }

        [DataMember]
        [Column("NAME")]
        public string NAME { get; set; }

        [DataMember]
        [Column("LINETYPE")]
        public int LINETYPE { get; set; }       
    }


}
