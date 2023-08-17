namespace CDC.Commerce.Runtime.InstitutionalCustomer.Model
{
    using System.Collections.Generic;

    public class CSDRebateMonthlyQuantity
    {
        public List<CSDRebateMonthlyQuantitySummaryList> CSDRebateMonthlyQuantitySummaryList { get; set; }
    }

    public class CSDRebateMonthlyQuantitySummaryList
    {
        public long Category { get; set; }
        public string LastTransactionDate { get; set; }
        public decimal LastTransactionQty { get; set; }
        public string Unit { get; set; }
        public decimal MonthlyQtyLimit { get; set; }
        public decimal RemainingQtyLimit { get; set; }
    }
}
