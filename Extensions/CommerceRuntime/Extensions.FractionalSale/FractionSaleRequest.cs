using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.FractionalSale
{
    [DataContract]
    public sealed class FractionSaleRequest : Request
    {        
        [DataMember]
        public List<ProductInformation> ProductsInformation { get; set; }
    }

    public class ProductInformation
    {
        public string RetailStoreId { get; set; }
        public long ProductId { get; set; }
        public string UnitOfMeasure { get; set; }
    }
}
