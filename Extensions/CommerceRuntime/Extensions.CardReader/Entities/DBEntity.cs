using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.CardReader.Entities
{
    public class DBEntity : CommerceEntity
    {
        public DBEntity() : base("CardReader")
        {

        }

        [DataMember]
        [Column("CDCISCARDBLOCKED")]
        public int CDCISCARDBLOCKED { get; set; }

        [DataMember]
        [Column("Return Value")]
        public int RETURNVALUE { get; set; }
    }
}
