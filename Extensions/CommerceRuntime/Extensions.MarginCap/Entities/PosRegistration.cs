using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

using System.Runtime.Serialization;
using System.Text;

namespace CDC.CommerceRuntime.Entities.DataModel
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
