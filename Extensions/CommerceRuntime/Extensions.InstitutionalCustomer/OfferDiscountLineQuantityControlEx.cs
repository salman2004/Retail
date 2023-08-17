namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Framework.Serialization;
    using Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine;
    using Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine.DiscountData;

    public class OfferDiscountLineQuantityControlEx : OfferDiscountLineQuantityControl
    {
        public new decimal QuantityLimit { get; private set; }

        private Dictionary<string, decimal> itemToQuantityMap;
        private Dictionary<int, decimal> itemGroupIndexToQuantityMap;

        public OfferDiscountLineQuantityControlEx(ValidationPeriod validationPeriod, decimal quantityLimit)
            : base(validationPeriod, Convert.ToInt32(Math.Ceiling(quantityLimit)))
        {
            this.QuantityLimit = quantityLimit;
            this.itemToQuantityMap = new Dictionary<string, decimal>();
            this.itemGroupIndexToQuantityMap = new Dictionary<int, decimal>();
        }

        public override bool BuildAndStreamlineLookups(DiscountableItemGroup[] discountableItemGroups, decimal[] remainingQuantities, PriceContext priceContext, HashSet<int> itemsWithOverlappingDiscounts, HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
        {
            bool status = base.BuildAndStreamlineLookups(discountableItemGroups, remainingQuantities, priceContext, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly);

            string allowedQtyMap = this.DiscountLines?.FirstOrDefault().Value?.MixAndMatchLineGroup ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(allowedQtyMap))
            {
                this.itemToQuantityMap = JsonHelper.Deserialize<Dictionary<string, decimal>>(allowedQtyMap);
            }

            if (!itemToQuantityMap.IsNullOrEmpty() && !discountableItemGroups.IsNullOrEmpty())
            {
                foreach (var item in discountableItemGroups)
                {
                    string itemDimensionId = (item.ItemId + item[0]?.InventoryDimensionId ?? string.Empty).Trim();

                    int index = discountableItemGroups.ToList().IndexOf(item);
                    decimal remainingQuantity = remainingQuantities[index];

                    if (this.itemToQuantityMap.TryGetValue(itemDimensionId, out decimal allowedQty))
                    {
                        if (allowedQty > remainingQuantity)
                        {
                            allowedQty = remainingQuantity;
                        }

                        this.itemToQuantityMap[itemDimensionId] -= allowedQty;
                        this.itemGroupIndexToQuantityMap.Add(index, allowedQty);
                    }
                }
            }

            return status;
        }

        public override void RemoveItemIndexGroupFromLookups(int itemGroupIndex)
        {
            // do nothing to avoid fractional quantity removal
        }

        public override AppliedDiscountApplication CreateAppliedDiscountApplication(DiscountableItemGroup[] discountableItemGroups, decimal[] remainingQuantities, IEnumerable<AppliedDiscountApplication> appliedDiscounts, DiscountApplication discountApplication, PriceContext priceContext)
        {
            AppliedDiscountApplication appliedDiscountApplication = base.CreateAppliedDiscountApplication(discountableItemGroups, remainingQuantities, appliedDiscounts, discountApplication, priceContext);

            int itemGroupIndex = appliedDiscountApplication?.DiscountApplication?.RetailDiscountLines?.FirstOrDefault()?.ItemGroupIndex ?? int.MinValue;
            this.itemGroupIndexToQuantityMap.TryGetValue(itemGroupIndex, out decimal allowedQty);

            if (appliedDiscountApplication != null && itemGroupIndex != int.MinValue)
            {
                appliedDiscountApplication.ItemQuantities[itemGroupIndex] = allowedQty;
                appliedDiscountApplication.DiscountApplication.ItemQuantities[itemGroupIndex] = allowedQty;
                appliedDiscountApplication.ItemGroupIndexToDiscountLineQuantitiesLookup[itemGroupIndex].FirstOrDefault().Quantity = allowedQty;

                if (this.itemGroupIndexToQuantityMap.ContainsKey(itemGroupIndex))
                {
                    this.itemGroupIndexToQuantityMap[itemGroupIndex] -= allowedQty;
                }
            }

            return appliedDiscountApplication;
        }
    }
}
