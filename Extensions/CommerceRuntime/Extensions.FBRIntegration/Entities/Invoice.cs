using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.FBRIntegration.Entities
{    
    public class InvoiceItem
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public string PCTCode { get; set; }
        public double TaxRate { get; set; }
        public double SaleValue { get; set; }
        public double TotalAmount { get; set; }
        public double TaxCharged { get; set; }
        public double Discount { get; set; }
        public double FurtherTax { get; set; }
        public int InvoiceType { get; set; }
        public object RefUSIN { get; set; }
    }

    public class Invoice
    {
        public string InvoiceNumber { get; set; }
        public int POSID { get; set; }
        public string USIN { get; set; }
        public string DateTime { get; set; }
        public string BuyerNTN { get; set; }
        public string BuyerCNIC { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhoneNumber { get; set; }
        public double TotalBillAmount { get; set; }
        public double TotalQuantity { get; set; }
        public double TotalSaleValue { get; set; }
        public double TotalTaxCharged { get; set; }
        public double Discount { get; set; }
        public double FurtherTax { get; set; }
        public int PaymentMode { get; set; }
        public object RefUSIN { get; set; }
        public int InvoiceType { get; set; }
        public List<InvoiceItem> Items { get; set; }
    }


}
