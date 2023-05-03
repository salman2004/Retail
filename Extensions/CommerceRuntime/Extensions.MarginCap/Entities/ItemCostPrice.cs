using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.CommerceRuntime.Entities.DataModel
{
    public class ItemCostPrice : CommerceEntity
    {
        private const string ITEMID = "ITEMID";
        public ItemCostPrice()
        : base("ItemCostPrice")
        {
        }       

        [DataMember]
        [Column(ITEMID)]
        public string OpenTime
        {
            get { return (string)this[ITEMID]; }
            set { this[ITEMID] = value; }
        }
    }
}
