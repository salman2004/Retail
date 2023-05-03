namespace CDC.Commerce.Runtime.InstitutionalCustomer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
    using Microsoft.Dynamics.Commerce.Runtime.Services;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Newtonsoft.Json;
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

            retailDiscounts = FilterMonthlyCapDiscounts(retailDiscounts);
            retailDiscounts = FilterMarginCapDiscounts(retailDiscounts);

            return retailDiscounts;
        }

        public new object ReadTenderDiscounts(object items, object priceGroups, DateTimeOffset minActiveDate, DateTimeOffset maxActiveDate, QueryResultSettings settings)
        {
            ReadOnlyCollection<TenderDiscountRule> tenderDiscounts = base.ReadTenderDiscounts(items, priceGroups, minActiveDate, maxActiveDate, settings) as ReadOnlyCollection<TenderDiscountRule>;

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

            return tenderDiscounts;
        }

        private ReadOnlyCollection<PeriodicDiscount> FilterMonthlyCapDiscounts(ReadOnlyCollection<PeriodicDiscount> retailDiscounts)
        {
            GetLoyaltyCardDetails(out string cardNumber, out decimal cardBalance, this.RequestContext, out DateTime lastTransactionDateTime);

            if (lastTransactionDateTime != DateTime.MinValue && lastTransactionDateTime > DateTime.Now)
            {
                this.MonthlyLimitUsed = decimal.Zero;
                return new List<PeriodicDiscount>().AsReadOnly();
            }

            if (retailDiscounts.IsNullOrEmpty())
            {
                return retailDiscounts;
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

            if (checkLoyaltyLimit && this.Transaction.LoyaltyCardId.Equals(cardNumber) && cardBalance > decimal.Zero)
            {
                Dictionary<string, decimal> itemPriceMap = new Dictionary<string, decimal>();

                //Sorting 
                GetItemGSTDetails(this.RequestContext, this.Transaction, out List<ExtensionsEntity> entities);
                foreach (var item in retailDiscounts)
                {
                    ExtensionsEntity entity = entities.Where(a => (a.GetProperty("InventoryDimensionId")?.ToString()?.Trim() ?? string.Empty) == item.InventoryDimensionId && (a.GetProperty("ItemId")?.ToString()?.Trim() ?? string.Empty) == item.ItemId).FirstOrDefault();
                    item.SetProperty("CDCTOPONCART", entity?.GetProperty("CDCTOPONCART") ?? decimal.Zero);
                    item.SetProperty("CDCPRICINGPRIORITY", entity?.GetProperty("CDCPRICINGPRIORITY") ?? decimal.Zero);
                    item.SetProperty("GrossProfit", entity?.GetProperty("GrossProfit") ?? decimal.Zero);

                }
                retailDiscounts = retailDiscounts.OrderByDescending(a => Convert.ToDecimal(a.GetProperty("CDCTOPONCART"))).ThenByDescending(x => Convert.ToDecimal(x.GetProperty("CDCPRICINGPRIORITY"))).ThenByDescending(z => Convert.ToDecimal(z.GetProperty("GrossProfit"))).AsReadOnly();
                //Sorting

                foreach (SalesLine salesLine in this.Transaction?.ActiveSalesLines)
                {
                    if (itemPriceMap.ContainsKey(string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)))
                    {
                        decimal quantity = retailDiscounts.Where(a => a.InventoryDimensionId == salesLine.InventoryDimensionId && a.ItemId == salesLine.ItemId)?.Where(b => b.OfferQuantityLimit > 0)?.FirstOrDefault()?.OfferQuantityLimit ?? decimal.Zero;
                        itemPriceMap[string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)] = itemPriceMap[string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId)] + (quantity != decimal.Zero ? quantity : salesLine.Quantity) * salesLine.Price;
                    }
                    else
                    {
                        itemPriceMap.Add(string.Format("{0}::{1}", salesLine.ItemId, salesLine.InventoryDimensionId), salesLine.Price * salesLine.Quantity);
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
                        if (lineTotal + cartTotal - earlierLineTotal > cardBalance)
                        {
                            continue;
                        }
                        else if (itemPriceTotal.ContainsKey(string.Format("{0}::{1}", retailDiscount.ItemId, retailDiscount.InventoryDimensionId)))
                        {
                            filteredRetailDiscounts.Add(retailDiscount);
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
