namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using System.Collections.Generic;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine;
    using Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine.DiscountData;

    public class DiscountPackageOfferLineLimitEx : IDiscountPackage
    {
        public ExtensiblePeriodicDiscountOfferType DiscountOfferType => ExtensiblePeriodicDiscountOfferType.OfferLineQuantityLimit;

        public DiscountBase CreateDiscount(PeriodicDiscount discountAndLine)
        {
            ThrowIf.Null(discountAndLine, "discountAndLine");
            if (discountAndLine.ThresholdApplyingLineQuantityLimit > decimal.Zero)
            {
                return new OfferDiscountLineQuantityControlEx(discountAndLine.ValidationPeriod, discountAndLine.ThresholdApplyingLineQuantityLimit);
            }
            else
            {
                return new OfferDiscountLineQuantityControl(discountAndLine.ValidationPeriod, discountAndLine.OfferQuantityLimit);
            }
        }

        public virtual void LoadDiscountDetails(Dictionary<string, DiscountBase> offerIdToDiscountMap, IPricingDataAccessor pricingDataManager, SalesTransaction transaction)
        {
        }
    }
}
