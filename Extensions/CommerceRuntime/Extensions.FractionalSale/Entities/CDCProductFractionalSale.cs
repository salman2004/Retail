using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Runtime.Serialization;

namespace CDC.Commerce.Runtime.FractionalSale.Entities
{
    class CDCProductFractionalSale : CommerceEntity
    {
        private const string IdColumn = "RECID";
        private const string CategoryColumn = "CATEGORY";
        private const string CategoryHierarchyColumn = "CATEGORYHIERARCHY";
        private const string ProductColumn = "PRODUCT";
        private const string VariantColumn = "VARIANT";
        private const string StoreNumberColumn = "STORENUMBER";
        private const string NameColumn = "NAME";
        private const string LineTypeColumn = "LINETYPE";

        public CDCProductFractionalSale() : base("CDCProductFractionalSale")
        {
        }

        [Key]
        [DataMember]
        [Column(IdColumn)]
        public long RECID
        {
            get { return (long)this[IdColumn]; }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        [Column(CategoryColumn)]
        public long CATEGORY
        {
            get { return (long)this[CategoryColumn]; }
            set { this[CategoryColumn] = value; }
        }

        [DataMember]
        [Column(ProductColumn)]
        public long PRODUCT
        {
            get { return (long)this[ProductColumn]; }
            set { this[ProductColumn] = value; }
        }

        [DataMember]
        [Column(VariantColumn)]
        public long VARIANT
        {
            get { return (long)this[VariantColumn]; }
            set { this[VariantColumn] = value; }
        }

        [DataMember]
        [Column(CategoryHierarchyColumn)]
        public long CATEGORYHIERARCHY
        {
            get { return (long)this[CategoryHierarchyColumn]; }
            set { this[CategoryHierarchyColumn] = value; }
        }

        [DataMember]
        [Column(StoreNumberColumn)]
        public string STORENUMBER
        {
            get { return (string)this[StoreNumberColumn]; }
            set { this[StoreNumberColumn] = value; }
        }

        [DataMember]
        [Column(NameColumn)]
        public string NAME
        {
            get { return (string)this[NameColumn]; }
            set { this[NameColumn] = value; }
        }

        [DataMember]
        [Column(LineTypeColumn)]
        public int LINETYPE
        {
            get { return (int)this[LineTypeColumn]; }
            set { this[LineTypeColumn] = value; }
        }
    }
}
