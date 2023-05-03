using System.Runtime.Serialization;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace CDC.Commerce.Runtime.MarginCap.Entities
{
    public class RetailStoreTable : CommerceEntity
    {
        public RetailStoreTable() : base("RetailStoreTable")
        {
        }

        [DataMember]
        [Column("CDCMARGINCAPPROTECTION")]
        public int CDCMARGINCAPPROTECTION { get; set; }
    }
}
