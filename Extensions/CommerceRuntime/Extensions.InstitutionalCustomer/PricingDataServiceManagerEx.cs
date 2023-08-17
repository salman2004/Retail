namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using CDC.Commerce.Runtime.InstitutionalCustomer.Model;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Framework.Serialization;
    using Microsoft.Dynamics.Commerce.Runtime.Services;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using PE = Microsoft.Dynamics.Commerce.Runtime.Services.PricingEngine;

    public class PricingDataServiceManagerEx : PricingDataServiceManager, PE.IPricingDataAccessorV2
    {
        public decimal MonthlyLimitUsed { get; private set; }
        protected SalesTransaction Transaction { get; private set; }

        public PricingDataServiceManagerEx(RequestContext requestContext, SalesTransaction transaction)
            : base(requestContext)
        {
            MonthlyLimitUsed = decimal.Zero;
            Transaction = transaction;
        }

        public override object ReadRetailDiscounts(object items, object priceGroups, DateTimeOffset minActiveDate, DateTimeOffset maxActiveDate, string currencyCode, QueryResultSettings settings)
        {
            ReadOnlyCollection<PeriodicDiscount> retailDiscounts = base.ReadRetailDiscounts(items, priceGroups, minActiveDate, maxActiveDate, currencyCode, settings) as ReadOnlyCollection<PeriodicDiscount>;

            retailDiscounts = SortDiscounts(retailDiscounts);
            retailDiscounts = FilterRebateQtyLimit(retailDiscounts);
            //retailDiscounts = FilterMonthlyCapDiscounts(retailDiscounts);
            retailDiscounts = FilterMarginCapDiscounts(retailDiscounts);
            return retailDiscounts;
        }

        public new object ReadTenderDiscounts(object items, object priceGroups, DateTimeOffset minActiveDate, DateTimeOffset maxActiveDate, QueryResultSettings settings)
        {
            ReadOnlyCollection<TenderDiscountRule> tenderDiscounts = base.ReadTenderDiscounts(items, priceGroups, minActiveDate, maxActiveDate, settings) as ReadOnlyCollection<TenderDiscountRule>;
            this.Transaction.SetProperty("isTenderDiscount", true);
            GetTenderDiscountOfferIds(this.RequestContext, out List<string> offerIds);
            GetTenderDiscountValue(this.RequestContext, out decimal maxDiscount);

            if (tenderDiscounts.Any(td => offerIds.Contains(td.OfferId)) && maxDiscount > decimal.Zero)
            {
                var offerTenderDiscounts = tenderDiscounts.GroupBy(td => td.OfferId);
                foreach (var offerTenderDiscount in offerTenderDiscounts)
                {
                    if (offerIds.Contains(offerTenderDiscount.Key))
                    {
                        var offerTenderDiscountTotal = decimal.Zero;
                        foreach (var item in offerTenderDiscount.GroupBy(td => td.ProductId).ToList())
                        {
                            offerTenderDiscountTotal += this.Transaction?.ActiveSalesLines?.Where(sl => sl.ProductId == item.Key)?.Sum(sl => (sl.Price * sl.Quantity) - sl.DiscountAmount) ?? decimal.Zero;
                        }

                        // Done for the purpose of rounding off amount to 1 using rounding method down
                        offerTenderDiscountTotal = Convert.ToDecimal((int)offerTenderDiscountTotal);

                        decimal otherTenderedAmount = this?.Transaction?.TenderLines?.Where(tl => tl.Status == TenderLineStatus.Committed)?.Sum(tl => tl.AmountInCompanyCurrency) ?? decimal.Zero;
                        offerTenderDiscountTotal = offerTenderDiscountTotal - otherTenderedAmount;

                        var offerTenderDiscountPct = offerTenderDiscount.FirstOrDefault().DiscountPercent;
                        var offerDiscountAmount = offerTenderDiscountTotal == decimal.Zero ? decimal.Zero : (offerTenderDiscountTotal * offerTenderDiscountPct) / 100;

                        if (maxDiscount < offerDiscountAmount)
                        {
                            offerTenderDiscountPct = (maxDiscount / offerTenderDiscountTotal) * 100;
                        }

                        foreach (var item in offerTenderDiscount.ToList())
                        {
                            item.DiscountPercent = offerTenderDiscountPct;
                        }
                    }
                }
            }


            if (this.Transaction.SalesLines.Where(p=>!p.IsVoided).Sum(p => (p.Quantity * p.Price)) >= 5000)
            {
                foreach(TenderDiscountRule td in tenderDiscounts)
                {
                    td.AmountThreshold = this.Transaction.AmountDue;
                }
            }
            return tenderDiscounts;
        }
        
        private ReadOnlyCollection<PeriodicDiscount> SortDiscounts(ReadOnlyCollection<PeriodicDiscount> retailDiscounts)
        {
            if (retailDiscounts.IsNullOrEmpty())
            {
                return retailDiscounts;
            }

            try
            {
                GetItemGSTDetails(this.RequestContext, this.Transaction, out List<ExtensionsEntity> entities);
                foreach (var item in retailDiscounts)
                {
                    ExtensionsEntity entity = entities.Where(a => (a.GetProperty("InventoryDimensionId")?.ToString()?.Trim() ?? string.Empty) == item.InventoryDimensionId && (a.GetProperty("ItemId")?.ToString()?.Trim() ?? string.Empty) == item.ItemId).FirstOrDefault();
                    item.SetProperty("CDCTOPONCART", entity?.GetProperty("CDCTOPONCART") ?? decimal.Zero);
                    item.SetProperty("CDCPRICINGPRIORITY", entity?.GetProperty("CDCPRICINGPRIORITY") ?? decimal.Zero);
                    item.SetProperty("GrossProfit", entity?.GetProperty("GrossProfit") ?? decimal.Zero);

                }
                retailDiscounts = retailDiscounts.OrderByDescending(a => Convert.ToDecimal(a.GetProperty("CDCTOPONCART"))).ThenByDescending(x => Convert.ToDecimal(x.GetProperty("CDCPRICINGPRIORITY"))).ThenByDescending(z => Convert.ToDecimal(z.GetProperty("GrossProfit"))).AsReadOnly();
                return retailDiscounts;
            }
            catch (Exception)
            {
                return retailDiscounts;
            }

        }

        private ReadOnlyCollection<PeriodicDiscount> FilterMonthlyCapDiscounts(ReadOnlyCollection<PeriodicDiscount> retailDiscounts)
        {
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, this.RequestContext, out DateTime lastTransactionDateTime);

            if (lastTransactionDateTime != DateTime.MinValue && lastTransactionDateTime > DateTime.Now)
            {
                this.MonthlyLimitUsed = decimal.Zero;
                return new List<PeriodicDiscount>().AsReadOnly();
            }

            if (string.IsNullOrWhiteSpace(this.Transaction.LoyaltyCardId)
                || string.IsNullOrWhiteSpace(cardNumber)
                || !this.Transaction.LoyaltyCardId.Equals(cardNumber)
                || !this.Transaction.AffiliationLoyaltyTierLines.Any(alt => alt.AffiliationType == RetailAffiliationType.Loyalty))
            {
                this.MonthlyLimitUsed = decimal.Zero;
                return retailDiscounts;
            }

            SalesAffiliationLoyaltyTier affiliationLoyaltyTier = this.Transaction.AffiliationLoyaltyTierLines.FirstOrDefault(alt => alt.AffiliationType == RetailAffiliationType.Loyalty);
            GetLoyaltyDetails(this.RequestContext, affiliationLoyaltyTier.AffiliationId, out decimal loyaltyLimit, out bool checkLoyaltyLimit);
            this.Transaction.SetProperty("checkLoyaltyLimit", checkLoyaltyLimit);

            if (!checkLoyaltyLimit)
            {
                this.MonthlyLimitUsed = decimal.Zero;
                return retailDiscounts;
            }

            if (retailDiscounts.IsNullOrEmpty())
            {
                return retailDiscounts;
            }

            if (checkLoyaltyLimit && this.Transaction.LoyaltyCardId.Equals(cardNumber) && cardBalance > decimal.Zero)
            {
                Dictionary<string, decimal> itemPriceMap = new Dictionary<string, decimal>();
                
                foreach (SalesLine salesLine in this.Transaction?.ActiveSalesLines)
                {
                    var discounts = retailDiscounts.Where(a => a.InventoryDimensionId == salesLine.InventoryDimensionId && a.ItemId == salesLine.ItemId);
                    if (itemPriceMap.ContainsKey(string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)))
                    {
                        if (discounts?.Any(rd => Convert.ToBoolean(rd.GetProperty("ISCDCREBATEQTYLIMIT"))) ?? false)
                        {
                            continue;
                        }
                        decimal quantity = discounts?.Where(b => b.OfferQuantityLimit > 0)?.FirstOrDefault()?.OfferQuantityLimit ?? decimal.Zero;
                        
                        if (quantity > decimal.Zero) /// when offer qty for qty threshold discount 
                        {
                            itemPriceMap[string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)] = (quantity != decimal.Zero ? quantity : salesLine.Quantity) * salesLine.Price;
                        }
                        else
                        {
                            itemPriceMap[string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)] = itemPriceMap[string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)] + (salesLine.Quantity * salesLine.Price);
                        }
                    }
                    else
                    {
                        if (discounts?.Any(rd => Convert.ToBoolean(rd.GetProperty("ISCDCREBATEQTYLIMIT"))) ?? false)
                        {
                            decimal quantity = decimal.Zero;
                            // read quantity from rebate JSON
                            var retailDiscount = discounts?.FirstOrDefault(rd => Convert.ToBoolean(rd.GetProperty("ISCDCREBATEQTYLIMIT")));
                            string rebateMixAndMatchGroup = retailDiscount?.MixAndMatchLineGroup ?? string.Empty;
                            if (JsonHelper.TryDeserialize<Dictionary<string, decimal>>(rebateMixAndMatchGroup, out var itemToQuantityMap)
                                && itemToQuantityMap.ContainsKey(salesLine.ItemId + salesLine.InventoryDimensionId))
                            {
                                quantity = itemToQuantityMap[salesLine.ItemId + salesLine.InventoryDimensionId];
                            }
                            else
                            {
                                quantity = decimal.Zero;
                            }

                            if (quantity > decimal.Zero)
                            {
                                itemPriceMap.Add(string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId), salesLine.Price * quantity);
                            }
                        }
                        else
                        {
                            itemPriceMap.Add(string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId), salesLine.Price * salesLine.Quantity);
                        }
                    }
                }

                decimal cartTotal = decimal.Zero;
                Dictionary<string, decimal> itemPriceTotal = new Dictionary<string, decimal>();

                List<PeriodicDiscount> filteredRetailDiscounts = new List<PeriodicDiscount>();
                foreach (var retailDiscount in retailDiscounts)
                {
                    if (itemPriceMap.TryGetValue(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId), out decimal lineTotal))
                    {
                        itemPriceTotal.TryGetValue(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId), out decimal earlierLineTotal);

                        if (cartTotal >= cardBalance && !(itemPriceTotal.ContainsKey(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId))))
                        {
                            continue; //TODO: Break can be used
                        }
                        else if (itemPriceTotal.ContainsKey(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId)))
                        {
                            if ((lineTotal + cartTotal - earlierLineTotal > cardBalance))
                            {
                                decimal remainingAmount = cardBalance - (cartTotal - earlierLineTotal);
                                retailDiscount.DiscountPercent = ((remainingAmount * (retailDiscount.DiscountPercent / 100) / lineTotal) * 100);
                                retailDiscount.SetProperty("PartialDiscountPCT", retailDiscount.DiscountPercent);
                                filteredRetailDiscounts.Add(retailDiscount);
                                continue;
                            }
                            else
                            {
                                
                                filteredRetailDiscounts.Add(retailDiscount);
                                continue;
                            }
                        }
                        else if ((lineTotal + cartTotal - earlierLineTotal > cardBalance))
                        {
                            decimal remainingAmount = cardBalance - cartTotal;
                            retailDiscount.DiscountPercent = ((remainingAmount * (retailDiscount.DiscountPercent / 100) / lineTotal) * 100);
                            itemPriceTotal.Add(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId), remainingAmount);
                            retailDiscount.SetProperty("PartialDiscountPCT", retailDiscount.DiscountPercent);
                            filteredRetailDiscounts.Add(retailDiscount);
                            cartTotal += remainingAmount;
                        }
                        else
                        {
                            cartTotal += lineTotal;
                            itemPriceTotal.Add(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId), lineTotal);
                            filteredRetailDiscounts.Add(retailDiscount);
                        }
                    }
                }

                return filteredRetailDiscounts.AsReadOnly();
            }
            else
            {
                this.MonthlyLimitUsed = decimal.Zero;
                return new List<PeriodicDiscount>().AsReadOnly();
            }
        }

        private ReadOnlyCollection<PeriodicDiscount> FilterMarginCapDiscounts(ReadOnlyCollection<PeriodicDiscount> retailDiscounts)
        {
            if (retailDiscounts.IsNullOrEmpty())
            {
                return retailDiscounts;
            }

            GetGrossMarginCapAffiliation(this.RequestContext, out List<string> affiliations);
            GetGrossMarginCap(this.RequestContext, out decimal grossMarginCap);

            if (!affiliations.IsNullOrEmpty() && !string.IsNullOrWhiteSpace(this.Transaction.CustomerId) && grossMarginCap > decimal.Zero)
            {
                QueryResultSettings querySettings = new QueryResultSettings(new PagingInfo(affiliations.Count(), 0), new SortingInfo());
                GetAffiliationsByNameDataRequest getAffiliationsByNameRequest = new GetAffiliationsByNameDataRequest(affiliations, querySettings) { RequestContext = this.RequestContext };
                EntityDataServiceResponse<Affiliation> getAffiliationsByNameResponse = this.RequestContext.ExecuteAsync<EntityDataServiceResponse<Affiliation>>(getAffiliationsByNameRequest).Result;

                if (getAffiliationsByNameResponse != null
                    && !getAffiliationsByNameResponse.PagedEntityCollection.IsNullOrEmpty()
                    && this.Transaction.AffiliationLoyaltyTierLines.Any(a => getAffiliationsByNameResponse.PagedEntityCollection.Any(pea => pea.RecordId == a.AffiliationId)))
                {
                    List<PeriodicDiscount> filteredRetailDiscounts = new List<PeriodicDiscount>();
                    foreach (var retailDiscount in retailDiscounts)
                    {
                        SalesLine line = this.Transaction.DiscountCalculableSalesLines.Where(sl => sl.ItemId == retailDiscount.ItemId && sl.InventoryDimensionId == retailDiscount.InventoryDimensionId).FirstOrDefault();
                        if (line != null)
                        {
                            decimal costPrice = GetCostPriceAsync(this.RequestContext, line);
                            decimal margin = CalculateGrossMargin(costPrice, line.Price);

                            if (margin >= grossMarginCap)
                            {
                                filteredRetailDiscounts.Add(retailDiscount);
                            }
                        }
                    }

                    if (filteredRetailDiscounts.IsNullOrEmpty())
                    {
                        return new List<PeriodicDiscount>().AsReadOnly();
                    }
                    else
                    {
                        return filteredRetailDiscounts.AsReadOnly();
                    }
                }
            }

            return retailDiscounts;
        }

        private ReadOnlyCollection<PeriodicDiscount> FilterRebateQtyLimit(ReadOnlyCollection<PeriodicDiscount> retailDiscounts)
        {
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, this.RequestContext, out DateTime lastTransactionDateTime);

            if(cardBalance <= 0)
            {
                return retailDiscounts;
            }
            List<PeriodicDiscount> filteredRetailDiscounts = this.CSDRebateMonthlyQuantityDiscounts(retailDiscounts?.Where(rd => rd?.ExtensiblePeriodicDiscountType == ExtensiblePeriodicDiscountOfferType.OfferLineQuantityLimit)?.ToList());
            if (filteredRetailDiscounts.IsNullOrEmpty())
            {
                return retailDiscounts;
            }

            CSDRebateMonthlyQuantity rebateMonthlyQuantity = this.GenerateCSDRebateMonthlyQuantity(out CSDRebateMonthlyQuantity rebateMonthlyQuantityCopy);

            if (rebateMonthlyQuantity == null || rebateMonthlyQuantity.CSDRebateMonthlyQuantitySummaryList.IsNullOrEmpty())
            {
                return retailDiscounts;
            }

            List<PeriodicDiscount> filteredDisocuntWithZeroCategoryLimit = new List<PeriodicDiscount>();
            Dictionary<long, decimal> categoryToMonthlyQuantityMap = rebateMonthlyQuantity?.CSDRebateMonthlyQuantitySummaryList?.ToDictionary(mq => mq.Category, mq => mq.RemainingQtyLimit);
            Dictionary<long, decimal> categoryToMonthlyQuantitySumMap = rebateMonthlyQuantity?.CSDRebateMonthlyQuantitySummaryList?.ToDictionary(mq => mq.Category, mq => decimal.Zero);

            Dictionary<string, decimal> itemToDiscountableQuantityMap = new Dictionary<string, decimal>();
            foreach (PeriodicDiscount periodicDiscount in filteredRetailDiscounts)
            {
                string itemDimensionId = (periodicDiscount.ItemId + periodicDiscount?.InventoryDimensionId ?? string.Empty).Trim();

                if (!itemToDiscountableQuantityMap.ContainsKey(itemDimensionId))
                {
                    long category = this.GetPeriodicDiscountProductCategory(periodicDiscount);

                    List<SalesLine> salesLines = this.Transaction?.ActiveSalesLines?.Where(sl => sl.ItemId == periodicDiscount.ItemId).ToList();
                    if (!string.IsNullOrWhiteSpace(periodicDiscount?.InventoryDimensionId))
                    {
                        salesLines = this.Transaction?.ActiveSalesLines?.Where(sl => sl.ItemId == periodicDiscount.ItemId && sl.InventoryDimensionId == periodicDiscount?.InventoryDimensionId).ToList();
                    }

                    foreach (SalesLine line in salesLines)
                    {
                        decimal sizeQuantity = decimal.One;
                        if (line?.Variant != null && !string.IsNullOrWhiteSpace(line?.Variant?.Size))
                        {
                            decimal.TryParse(Regex.Match(line?.Variant?.Size, @"(\d+(\.\d+)?)|(\.\d+)").Value, out sizeQuantity);

                            if (sizeQuantity > decimal.Zero)
                            {
                                string sizeUnit = this.GetTheUnit(new string(line?.Variant?.Size?.Where(char.IsLetter).ToArray()));
                                if (string.IsNullOrWhiteSpace(sizeUnit))
                                {
                                    sizeUnit = this.GetTheUnit(line?.UnitOfMeasureSymbol);
                                }

                                string categoryUnit = this.GetTheUnit(rebateMonthlyQuantity?.CSDRebateMonthlyQuantitySummaryList?.FirstOrDefault(ml => ml.Category == category)?.Unit ?? string.Empty);
                                if (string.IsNullOrWhiteSpace(categoryUnit))
                                {
                                    categoryUnit = this.GetTheUnit(line?.UnitOfMeasureSymbol);
                                }

                                if (sizeUnit != categoryUnit)
                                {
                                    List<ItemUnitConversion> itemUnitConversions = new List<ItemUnitConversion>()
                                    {
                                        new ItemUnitConversion()
                                        {
                                            ItemId = line?.ItemId,
                                            FromUnitOfMeasure = sizeUnit,
                                            ProductVariantId = line?.Variant?.DistinctProductVariantId ?? Convert.ToInt64(decimal.Zero),
                                            ToUnitOfMeasure = categoryUnit
                                        }
                                    };
                                    GetUnitOfMeasureConversionDataRequest getUnitOfMeasureConversionDataRequest = new GetUnitOfMeasureConversionDataRequest(itemUnitConversions, QueryResultSettings.SingleRecord) { RequestContext = this.RequestContext };
                                    GetUnitOfMeasureConversionDataResponse getUnitOfMeasureConversionDataResponse = this.RequestContext.Execute<GetUnitOfMeasureConversionDataResponse>(getUnitOfMeasureConversionDataRequest);

                                    if (getUnitOfMeasureConversionDataResponse != null && !getUnitOfMeasureConversionDataResponse.UnitConversions.IsNullOrEmpty())
                                    {
                                        UnitOfMeasureConversion unitOfMeasureConversion = getUnitOfMeasureConversionDataResponse.UnitConversions.First();
                                        sizeQuantity = sizeQuantity * unitOfMeasureConversion.Factor;
                                    }
                                }
                            }
                            else
                            {
                                sizeQuantity = decimal.One;
                            }
                        }

                        decimal itemDiscountableQuantity = line.Quantity;

                        categoryToMonthlyQuantityMap.TryGetValue(category, out decimal categoryLimit);
                        if (categoryLimit > decimal.Zero)
                        {
                            decimal allowedQty = line.Quantity * sizeQuantity;
                            if (allowedQty > categoryLimit)
                            {
                                itemDiscountableQuantity = line.Quantity * (categoryLimit / allowedQty);
                                allowedQty = categoryLimit;
                            }

                            if (itemToDiscountableQuantityMap.ContainsKey(itemDimensionId))
                            {
                                itemToDiscountableQuantityMap[itemDimensionId] += Math.Round(itemDiscountableQuantity, 2);
                            }
                            else
                            {
                                itemToDiscountableQuantityMap.Add(itemDimensionId, Math.Round(itemDiscountableQuantity, 2));
                            }

                            categoryToMonthlyQuantityMap[category] -= allowedQty;
                            categoryToMonthlyQuantitySumMap[category] += allowedQty;
                            
                            if (rebateMonthlyQuantityCopy.CSDRebateMonthlyQuantitySummaryList?.Where(rmq => rmq.Category == category)?.Count() > decimal.Zero)
                            {
                                CSDRebateMonthlyQuantitySummaryList rebateMonthlyQuantitySummary = rebateMonthlyQuantityCopy.CSDRebateMonthlyQuantitySummaryList.Where(rmq => rmq.Category == category).FirstOrDefault();
                                rebateMonthlyQuantitySummary.LastTransactionQty = categoryToMonthlyQuantitySumMap[category];
                                rebateMonthlyQuantitySummary.RemainingQtyLimit = categoryToMonthlyQuantityMap[category];
                                rebateMonthlyQuantitySummary.LastTransactionDate = DateTime.Now.ToString();
                            }

                        }
                        else
                        {
                            if (filteredDisocuntWithZeroCategoryLimit.Where(pd => pd == periodicDiscount).IsNullOrEmpty())
                            {
                                filteredDisocuntWithZeroCategoryLimit.Add(periodicDiscount);
                            }
                        }
                    }
                }
            }

            if (!itemToDiscountableQuantityMap.IsNullOrEmpty())
            {
                if (this.Transaction.IsPropertyDefined("RebateQtyLimit"))
                {
                    this.Transaction.SetProperty("SaveRebateQtyLimit", JsonConvert.SerializeObject(rebateMonthlyQuantityCopy));
                }

                foreach (PeriodicDiscount periodicDiscount in filteredRetailDiscounts)
                {
                    periodicDiscount.ThresholdApplyingLineQuantityLimit = itemToDiscountableQuantityMap?.Sum(item => item.Value) ?? decimal.Zero;
                    periodicDiscount.MixAndMatchLineGroup = JsonHelper.Serialize(itemToDiscountableQuantityMap);
                }

                List<PeriodicDiscount> updatedDiscounts = new List<PeriodicDiscount>();
                var offerId = filteredRetailDiscounts.FirstOrDefault().OfferId;
                foreach (PeriodicDiscount retailDiscount in retailDiscounts)
                {
                    if (filteredRetailDiscounts.Any(fd => fd.OfferId == retailDiscount.OfferId && fd.ItemId == retailDiscount.ItemId && fd.InventoryDimensionId == retailDiscount.InventoryDimensionId && fd.DiscountLineNumber == retailDiscount.DiscountLineNumber))
                    {
                        //Remove already added overlapping discount with rebate
                        if (updatedDiscounts.Any(fd => fd.ItemId == retailDiscount.ItemId && fd.InventoryDimensionId == retailDiscount.InventoryDimensionId))
                        {
                            var removeDiscounts = updatedDiscounts.Where(fd => fd.ItemId == retailDiscount.ItemId && fd.InventoryDimensionId == retailDiscount.InventoryDimensionId && !Convert.ToBoolean(fd?.GetProperty("ISCDCREBATEQTYLIMIT") ?? false)).ToList();
                            updatedDiscounts.RemoveRange(removeDiscounts);
                        }
                        
                        PeriodicDiscount updatedDiscount = filteredRetailDiscounts.Where(fd => fd.OfferId == retailDiscount.OfferId && fd.ItemId == retailDiscount.ItemId && fd.InventoryDimensionId == retailDiscount.InventoryDimensionId && fd.DiscountLineNumber == retailDiscount.DiscountLineNumber).FirstOrDefault();
                        updatedDiscounts.Add(updatedDiscount);
                    }
                    else
                    {
                        //Add only non over lapping discount with rebate qty
                        if (!updatedDiscounts.Any(fd => fd.ItemId == retailDiscount.ItemId && fd.InventoryDimensionId == retailDiscount.InventoryDimensionId && fd.OfferId == offerId))
                         {
                             updatedDiscounts.Add(retailDiscount);
                         }

                       // updatedDiscounts.Add(retailDiscount);

                    }
                }
               
                
                
                return updatedDiscounts.AsReadOnly();
            }
            else
            {
                try
                {
                    if (!filteredDisocuntWithZeroCategoryLimit.IsNullOrEmpty())
                    {
                        List<PeriodicDiscount> periodicDiscounts = retailDiscounts.ToList();
                        periodicDiscounts.RemoveRange(filteredDisocuntWithZeroCategoryLimit);
                        return new ReadOnlyCollection<PeriodicDiscount>(periodicDiscounts);
                    }
                }
                catch (Exception) { }

                return retailDiscounts;
            }
        }

        private string GetTheUnit(string unit)
        {
            switch (unit.ToUpper().Trim())
            {
                case "KG":
                case "KG-L":
                case "KG-M":
                case "KG-R/F":
                case "KG-R/P":
                    return "KG";
                case "GM":
                case "GM-PC":
                case "GM-PCS":
                case "GM-L":
                case "GM-M":
                    return "GM";
                case "LTR":
                case "LITRE":
                    return "LTR";
                case "PIECES":
                case "PCS":
                    return "PCS";
                case "MILLILITRE":
                case "ML":
                    return "ML";
                case "EA":
                    return "Eaches";
                default:
                    return unit.ToUpper().Trim();
            }
        }

        private long GetPeriodicDiscountProductCategory(PeriodicDiscount periodicDiscount)
        {
            long category = Convert.ToInt64(periodicDiscount.GetProperty("CATEGORYID"));
            if (category <= decimal.Zero)
            {
                using (DatabaseContext databaseContext = new DatabaseContext(this.RequestContext))
                {
                    var query = new SqlQuery
                    {
                        QueryString = @"SELECT GML.CATEGORY FROM AX.ECORESPRODUCT ERP
                                        INNER JOIN AX.ECORESPRODUCTCATEGORY ERC ON ERC.PRODUCT = ERP.RECID
                                        INNER JOIN AX.RETAILGROUPMEMBERLINE GML ON GML.CATEGORY = ERC.CATEGORY
                                        INNER JOIN AX.RETAILPERIODICDISCOUNTLINE PDL ON PDL.RETAILGROUPMEMBERLINE = GML.RECID
                                        WHERE PDL.OFFERID = @OFFERID AND ERP.DISPLAYPRODUCTNUMBER = @DISPLAYPRODUCTNUMBER AND PDL.DATAAREAID = @DATAAREAID"
                    };

                    query.Parameters["@OFFERID"] = periodicDiscount.OfferId;
                    query.Parameters["@DISPLAYPRODUCTNUMBER"] = periodicDiscount.ItemId;
                    query.Parameters["@DATAAREAID"] = this.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                    try
                    {
                        var categoryDetail = databaseContext.ReadEntity<ExtensionsEntity>(query);
                        category = Convert.ToInt64(Convert.ToString(categoryDetail?.FirstOrDefault()?.GetProperty("CATEGORY") ?? decimal.Zero));
                    }
                    catch (Exception ex)
                    {
                        category = Convert.ToInt64(decimal.Zero);
                    }
                }
            }

            return category;
        }

        private CSDRebateMonthlyQuantity GenerateCSDRebateMonthlyQuantity(out CSDRebateMonthlyQuantity rebateMonthlyQuantityCopy)
        {
            rebateMonthlyQuantityCopy = new CSDRebateMonthlyQuantity();
            CSDRebateMonthlyQuantity rebateMonthlyQuantity;
            try
            {
                if (this.RequestContext.Runtime.Configuration.IsMasterDatabaseConnectionString)
                {
                    rebateMonthlyQuantity = JsonConvert.DeserializeObject<CSDRebateMonthlyQuantity>(this.Transaction.GetProperty("RebateQtyLimit")?.ToString() ?? string.Empty);
                    rebateMonthlyQuantityCopy = JsonConvert.DeserializeObject<CSDRebateMonthlyQuantity>(this.Transaction.GetProperty("RebateQtyLimit")?.ToString() ?? string.Empty);
                }
                else
                {
                    rebateMonthlyQuantity = GenerateOfflineCSDRebateMonthlyQuantity();
                    rebateMonthlyQuantityCopy = GenerateOfflineCSDRebateMonthlyQuantity();
                }
                return ResetRebateMonthlyQuantity(this.RequestContext, rebateMonthlyQuantity);
            }
            catch (Exception)
            {
                return new CSDRebateMonthlyQuantity();
            }
        }

        private CSDRebateMonthlyQuantity GenerateOfflineCSDRebateMonthlyQuantity()
        {
            CSDRebateMonthlyQuantity rebateMonthlyQuantity;
            using (DatabaseContext databaseContext = new DatabaseContext(this.RequestContext))
            {
                SqlQuery query = new SqlQuery
                {
                    QueryString = $@"SELECT * from [ext].[CSDREBATEMONTHLYQTYDISCOUNTLIMITSETUP]"
                };

                try
                {
                    List<ExtensionsEntity> extensionsEntities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                    if (extensionsEntities.Count > decimal.Zero)
                    {
                        rebateMonthlyQuantity = new CSDRebateMonthlyQuantity();
                        rebateMonthlyQuantity.CSDRebateMonthlyQuantitySummaryList = ConvertRebateMonthlyQuantitySummaryList(extensionsEntities);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    rebateMonthlyQuantity = new CSDRebateMonthlyQuantity();
                }
            }
            return rebateMonthlyQuantity;

        }

        private List<CSDRebateMonthlyQuantitySummaryList> ConvertRebateMonthlyQuantitySummaryList(List<ExtensionsEntity> extensionsEntities)
        {
            List<CSDRebateMonthlyQuantitySummaryList> CSDRebateMonthlyQuantitySummaryList = new List<CSDRebateMonthlyQuantitySummaryList>();
            CSDRebateMonthlyQuantitySummaryList rebateMonthlyQuantitySummaryList;
            foreach (var item in extensionsEntities)
            {
                rebateMonthlyQuantitySummaryList = new CSDRebateMonthlyQuantitySummaryList();
                rebateMonthlyQuantitySummaryList.Category = Convert.ToInt64(item?.GetProperty("Category")?.ToString() ?? decimal.Zero.ToString());
                rebateMonthlyQuantitySummaryList.Unit = item?.GetProperty("Unit")?.ToString() ?? string.Empty;
                rebateMonthlyQuantitySummaryList.RemainingQtyLimit = Convert.ToDecimal(item?.GetProperty("MONTHLYQUANTITYLIMIT")?.ToString() ?? decimal.Zero.ToString());
                rebateMonthlyQuantitySummaryList.MonthlyQtyLimit = Convert.ToDecimal(item?.GetProperty("MONTHLYQUANTITYLIMIT")?.ToString() ?? decimal.Zero.ToString());
                rebateMonthlyQuantitySummaryList.LastTransactionDate = DateTime.Now.ToString();
                CSDRebateMonthlyQuantitySummaryList.Add(rebateMonthlyQuantitySummaryList);
            }
            return CSDRebateMonthlyQuantitySummaryList;
        }

        private CSDRebateMonthlyQuantity ResetRebateMonthlyQuantity(RequestContext context, CSDRebateMonthlyQuantity rebateMonthlyQuantity)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string resetRebateMonthlyQuantityOffsetDate = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("ResetRebateMonthlyQuantityOffsetDate").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            foreach (CSDRebateMonthlyQuantitySummaryList cSDRebateMonthlyQuantitySummary in rebateMonthlyQuantity.CSDRebateMonthlyQuantitySummaryList)
            {
                DateTime.TryParse(cSDRebateMonthlyQuantitySummary.LastTransactionDate, out DateTime lastTransactionDate);
                cSDRebateMonthlyQuantitySummary.RemainingQtyLimit = ResetRebateMonthlyQuantityHelper(cSDRebateMonthlyQuantitySummary.RemainingQtyLimit, cSDRebateMonthlyQuantitySummary.MonthlyQtyLimit, resetRebateMonthlyQuantityOffsetDate, lastTransactionDate);
            }
            return rebateMonthlyQuantity;
        }

        private decimal ResetRebateMonthlyQuantityHelper(decimal remainingQuantityLimit, decimal monthlyQuantityLimit, string ResetRebateMonthlyQuantityOffsetDate, DateTime lastTransactionDateTime)
        {
            try
            {
                int num = Convert.ToInt32(ResetRebateMonthlyQuantityOffsetDate);
                if (DateTime.Now.Month != lastTransactionDateTime.Month && lastTransactionDateTime.Day <= num)
                {
                    return monthlyQuantityLimit;
                }
                if (DateTime.Now.Month == lastTransactionDateTime.Month && DateTime.Now.Day >= num && lastTransactionDateTime.Day < num)
                {
                    return monthlyQuantityLimit;
                }
                if (DateTime.Now.Month != lastTransactionDateTime.Month && DateTime.Now.Day >= num)
                {
                    return monthlyQuantityLimit;
                }
                return remainingQuantityLimit;
            }
            catch (Exception)
            {
                return remainingQuantityLimit;
            }
        }

        private List<PeriodicDiscount> CSDRebateMonthlyQuantityDiscounts(List<PeriodicDiscount> periodicDiscounts)
        {
            string[] offerIds = periodicDiscounts.Select(pd => pd.OfferId).Distinct().ToArray();
            List<PeriodicDiscount> filteredRetailDiscounts = new List<PeriodicDiscount>();

            using (DatabaseContext databaseContext = new DatabaseContext(this.RequestContext))
            {
                SqlQuery query = new SqlQuery
                {
                    QueryString = $@"SELECT OFFERID, CDCREBATEQTYLIMIT FROM [ext].RETAILPERIODICDISCOUNT RPD WHERE RPD.OFFERID IN ({string.Join(",", offerIds.Select(offerid => "'" + offerid + "'"))})"
                };

                try
                {
                    List<string> eligibleOfferIds = databaseContext.ReadEntity<ExtensionsEntity>(query).Where(e => Convert.ToBoolean(e.GetProperty("CDCREBATEQTYLIMIT"))).Select(e => Convert.ToString(e.GetProperty("OFFERID"))).ToList();
                    filteredRetailDiscounts = periodicDiscounts.Where(pd => eligibleOfferIds.Contains(pd.OfferId)).ToList();

                    foreach (var filteredRetailDiscount in filteredRetailDiscounts)
                    {
                        filteredRetailDiscount.SetProperty("ISCDCREBATEQTYLIMIT", true);
                    }
                }
                catch (Exception)
                {
                    filteredRetailDiscounts = new List<PeriodicDiscount>();
                }
            }

            return filteredRetailDiscounts;
        }

        private decimal CalculateGrossMargin(decimal costPrice, decimal sellPrice)
        {
            if (costPrice <= decimal.Zero)
            {
                return decimal.Zero;
            }
            else
            {
                return ((sellPrice - costPrice) / sellPrice) * 100;
            }
        }

        private decimal GetCostPriceAsync(RequestContext context, SalesLine item)
        {
            decimal costPrice = decimal.Zero;
            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CostPrice"),
                    From = "CDCPRODUCTVARIANTCOSTPRICE",
                    Where = "DATAAREAID = @dataAreaId AND ITEMID = @itemId AND CONFIGID = @configId AND INVENTLOCATIONID = @inventLocationId AND INVENTCOLORID = @inventColorId AND INVENTSTYLEID = @inventStyleId AND INVENTSIZEID = @inventSizeId",
                    OrderBy = "CostPrice"
                };

                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@itemId"] = item.ItemId;
                query.Parameters["@inventLocationId"] = item.InventoryLocationId;
                query.Parameters["@inventStyleId"] = item.Variant?.StyleId ?? string.Empty;
                query.Parameters["@inventColorId"] = item.Variant?.ColorId ?? string.Empty;
                query.Parameters["@inventSizeId"] = item.Variant?.SizeId ?? string.Empty;
                query.Parameters["@configId"] = item.Variant?.ConfigId ?? string.Empty;

                try
                {
                    var itemCostPrice = databaseContext.ReadEntity<ExtensionsEntity>(query);
                    costPrice = Convert.ToDecimal(Convert.ToString(itemCostPrice?.FirstOrDefault()?.GetProperty("COSTPRICE") ?? decimal.Zero));
                }
                catch (Exception)
                {
                    costPrice = decimal.Zero;
                }
            }

            return costPrice;
        }

        private void GetTenderDiscountOfferIds(RequestContext context, out List<string> offerIds)
        {
            offerIds = new List<string>();

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountOfferIds = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTHEADER").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountOfferIds))
            {
                offerIds = tenderDiscountOfferIds.Split(';').ToList();
            }
        }

        private void GetTenderDiscountValue(RequestContext context, out decimal maxDiscount)
        {
            maxDiscount = decimal.Zero;

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("MINDISCOUNTVALUE").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountValue) && decimal.TryParse(tenderDiscountValue, out decimal value))
            {
                maxDiscount = value;
            }
        }

        private void GetGrossMarginCapAffiliation(RequestContext context, out List<string> affiliations)
        {
            affiliations = new List<string>();

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string grossMarginCapAffiliation = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("GrossMarginCapAffiliation").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(grossMarginCapAffiliation))
            {
                affiliations = grossMarginCapAffiliation.Split(';').ToList();
            }
        }

        private void GetGrossMarginCap(RequestContext context, out decimal grossMarginCap)
        {
            grossMarginCap = decimal.Zero;

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string tenderDiscountValue = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("GrossMarginCap").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(tenderDiscountValue) && decimal.TryParse(tenderDiscountValue, out decimal value))
            {
                grossMarginCap = value;
            }
        }

        private void GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, RequestContext context, out DateTime lastTransactionDateTime)
        {
            cardNumber = this.Transaction?.GetProperty("CSDCardNumber")?.ToString()?.Trim() ?? string.Empty;
            decimal.TryParse(this.Transaction?.GetProperty("CSDCardBalance")?.ToString()?.Trim() ?? string.Empty, out cardBalance);
            decimal.TryParse(this.Transaction?.GetProperty("CSDOldCardBalance")?.ToString()?.Trim() ?? string.Empty, out decimal oldCardBalance);
            DateTime.TryParse(this.Transaction?.GetProperty("CSDCardResetDateTime")?.ToString() ?? string.Empty, out DateTime resetBalanceDateTime);

            if (!string.IsNullOrEmpty(this.Transaction.LoyaltyCardId) && cardNumber != string.Empty && resetBalanceDateTime > DateTime.Now)
            {
                cardBalance = oldCardBalance;
                throw new CommerceException("Microsoft_Dynamics_Commerce_30104", "Card Balance")
                {
                    LocalizedMessage = "Card balance was reset in future date time",
                    LocalizedMessageParameters = new object[] { }
                };
            }
            cardBalance = ResetCardBalance(cardBalance, context, out lastTransactionDateTime);
        }

        private decimal ResetCardBalance(decimal orignalCardBalance, RequestContext context, out DateTime lastTransactionDateTime)
        {
            decimal cardBalance = decimal.MinValue;
            lastTransactionDateTime = DateTime.MaxValue;

            DateTime.TryParse(this.Transaction?.GetProperty("CSDlastTransactionDateTime")?.ToString()?.Trim() ?? DateTime.MinValue.ToString(), out lastTransactionDateTime);
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string cardBalanceOffsetDay = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), ("ResetCardBalanceOffsetDay").ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
            try
            {
                int day = Convert.ToInt32(cardBalanceOffsetDay);
                if (DateTime.Now.Month != lastTransactionDateTime.Month && lastTransactionDateTime.Day <= day)
                {
                    decimal.TryParse(this.Transaction?.GetProperty("CSDCardLimit")?.ToString()?.Trim() ?? string.Empty, out cardBalance);
                }
                else if (DateTime.Now.Month == lastTransactionDateTime.Month && DateTime.Now.Day >= day && lastTransactionDateTime.Day < day)
                {
                    decimal.TryParse(this.Transaction?.GetProperty("CSDCardLimit")?.ToString()?.Trim() ?? string.Empty, out cardBalance);
                }
                else if (DateTime.Now.Month != lastTransactionDateTime.Month && DateTime.Now.Day >= day)
                {
                    decimal.TryParse(this.Transaction?.GetProperty("CSDCardLimit")?.ToString()?.Trim() ?? string.Empty, out cardBalance);
                }
                else
                {
                    return orignalCardBalance;
                }
            }
            catch (Exception)
            {
                return orignalCardBalance;
            }
            this.Transaction.SetProperty("CSDCardBalance", cardBalance.ToString());
            this.Transaction.SetProperty("CSDCardResetDateTime", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            this.Transaction.SetProperty("CSDOldCardBalance", orignalCardBalance);
            return cardBalance;
        }

        private void GetLoyaltyDetails(RequestContext context, long affiliationId, out decimal loyaltyLimit, out bool checkLoyaltyLimit)
        {
            loyaltyLimit = decimal.Zero;
            checkLoyaltyLimit = false;

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CDCPROTECTMONTHLYCAT", "CDCMONTHLYCATLIMIT"),
                    From = "RETAILAFFILIATION",
                    Where = "RECID = @affiliationId"
                };

                query.Parameters["@affiliationId"] = affiliationId;

                try
                {
                    var loyaltyDetail = databaseContext.ReadEntity<ExtensionsEntity>(query);
                    loyaltyLimit = Convert.ToDecimal(Convert.ToString(loyaltyDetail?.FirstOrDefault()?.GetProperty("CDCMONTHLYCATLIMIT") ?? decimal.Zero));
                    checkLoyaltyLimit = Convert.ToBoolean(Convert.ToInt32(Convert.ToString(loyaltyDetail?.FirstOrDefault()?.GetProperty("CDCPROTECTMONTHLYCAT") ?? decimal.Zero)));

                    if (!checkLoyaltyLimit)
                    {
                        this.Transaction.SetProperty("CSDMonthlyLimitUsed", "00000");
                        this.Transaction.SetProperty("CSDCardBalance", "00000");
                        this.Transaction.SetProperty("EmployeeCreditLimit", "00000");
                        this.MonthlyLimitUsed = decimal.Zero;
                    }
                }
                catch (Exception)
                {
                    loyaltyLimit = decimal.Zero;
                    checkLoyaltyLimit = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transaction"></param>
        /// <param name="entities"></param>
        private void GetItemGSTDetails(RequestContext context, SalesTransaction transaction, out List<ExtensionsEntity> entities)
        {
            if (transaction == null || transaction.ActiveSalesLines.IsNullOrEmpty())
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"SELECT DISTINCT IT.ITEMID, IT.CDCGSTTYPE, IT.CDCTOPONCART, DP.CDCPRICINGPRIORITY,C1.COSTPRICE, C1.INVENTCOLORID, C1.CONFIGID, C1.INVENTSTYLEID, C1.INVENTSIZEID, C1.INVENTLOCATIONID FROM [ax].INVENTDIMCOMBINATION IDM FULL OUTER JOIN [ext].[INVENTTABLE] IT on IT.ITEMID = IDM.ITEMID FULL OUTER JOIN[ext].[CDCGSTTYPEDISCOUNTPRIORITY] DP ON IT.CDCGSTTYPE = DP.CDCGSTTYPE FULL OUTER JOIN ext.CDCPRODUCTVARIANTCOSTPRICE C1 on C1.ITEMID = IT.ITEMID WHERE IDM.INVENTDIMID IN({string.Join(",", transaction.SalesLines.Select(sl => "'" + sl.InventoryDimensionId + "'"))}) AND IT.DATAAREAID = @dataAreaId AND C1.INVENTLOCATIONID = @inventLocationId  order by CDCTOPONCART desc, CDCPRICINGPRIORITY desc";
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@inventLocationId"] = context.GetDeviceConfiguration().InventLocationId;

                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                    entities = CalculateGrossProfit(entities);
                }
                catch (Exception)
                {
                    entities = new List<ExtensionsEntity>();
                }
            }
        }
        
        public List<ExtensionsEntity> CalculateGrossProfit(List<ExtensionsEntity> entities)
        {
            try
            {
                foreach (var item in this.Transaction.SalesLines)
                {
                    ExtensionsEntity entity = entities.Where(sl => item.ItemId.Equals(Convert.ToString(sl.GetProperty("ITEMID") ?? string.Empty))
                                     && item.Variant.ColorId == ((sl.GetProperty("INVENTCOLORID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTCOLORID")) : null)
                                     && item.Variant.StyleId == ((sl.GetProperty("INVENTSTYLEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTSTYLEID")) : null)
                                     && item.Variant.SizeId == ((sl.GetProperty("INVENTSIZEID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("INVENTSIZEID")) : null)
                                     && item.Variant.ConfigId == ((sl.GetProperty("CONFIGID")?.ToString()?.Trim() ?? string.Empty) != string.Empty ? Convert.ToString(sl.GetProperty("CONFIGID")) : null)
                                     ).FirstOrDefault();

                    if (entity != null)
                    {
                        entity.SetProperty("InventoryDimensionId", item.InventoryDimensionId);
                        entity.SetProperty("GrossProfit", CalculateGrossMargin(Convert.ToDecimal(entity?.GetProperty("COSTPRICE") ?? decimal.Zero), item?.Price ?? decimal.Zero));

                        item.SetProperty("CDCTOPONCART", entity.GetProperty("CDCTOPONCART")?.ToString()?.Trim() ?? string.Empty);
                        item.SetProperty("CDCPRICINGPRIORITY", entity.GetProperty("CDCPRICINGPRIORITY")?.ToString()?.Trim() ?? string.Empty);
                        item.SetProperty("GrossProfit", entity.GetProperty("GrossProfit")?.ToString()?.Trim() ?? string.Empty);

                    }

                }
                return entities.OrderByDescending(a => a.GetProperty("CDCTOPONCART")).ThenByDescending(x => x.GetProperty("CDCPRICINGPRIORITY")).ThenByDescending(z => z.GetProperty("GrossProfit")).ToList();
            }
            catch (Exception ex)
            {
                RetailLogger.Log.AxGenericErrorEvent(string.Format("Error when sorting: {0}", ex.Message));
                return entities;
            }
        }

    }
}
