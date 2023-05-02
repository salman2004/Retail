namespace CDC.Commerce.Runtime.MarginCap.RequestHandlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CDC.Commerce.Runtime.MarginCap.Entities;
    using CDC.CommerceRuntime.Entities.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Newtonsoft.Json;

    public class MarginCapService : IRequestHandlerAsync
    {

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CalculateDiscountsServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {

            GetMarginCapOnProductAndProductCategory capOnProductAndProductCategory;
            GetMarginCapOnStoreAndLoyaltyProgram getMarginCapOnStoreAndLoyalty;
            CalculateDiscountsServiceRequest discountsServiceRequest;
            GetPriceServiceResponse getPriceServiceResponse;

            bool excludeDiscount, isMarginCapEnabledOnProductAndProductCategory, isMarginCapEnabledForStoreAndLoyaltyProgram = false;
            decimal marginCapPercentageOnStoreAndEntity, marginCapPercentageOnProductAndProductCategory = 0.00M;

            discountsServiceRequest = (CalculateDiscountsServiceRequest)request;
            discountsServiceRequest.Transaction.SetProperty("CSDstoreId", request.RequestContext.GetDeviceConfiguration().StoreNumber);
            getPriceServiceResponse = (GetPriceServiceResponse)await ExecuteBaseRequestAsync(request);

            if (discountsServiceRequest.Transaction.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).Count() == 0)
            {
                return getPriceServiceResponse;
            }

            getMarginCapOnStoreAndLoyalty = GetMarginCapOnStoreAndLoyaltyProgram(request);
            isMarginCapEnabledForStoreAndLoyaltyProgram = Convert.ToBoolean(Convert.ToInt32(getMarginCapOnStoreAndLoyalty?.GetProperty("ISMARGINCAPALLOWEDONSTOREANDLOYALTY")?.ToString() ?? decimal.Zero.ToString()));
            marginCapPercentageOnStoreAndEntity = Convert.ToDecimal(getMarginCapOnStoreAndLoyalty?.GetProperty("MARGINCAPPERCENTAGE")?.ToString() ?? decimal.Zero.ToString());
            GetTenderDiscountOfferIds(request.RequestContext, out List<string> offerIds);

            if (!isMarginCapEnabledForStoreAndLoyaltyProgram)
            {
                return getPriceServiceResponse;
            }

            foreach (var item in getPriceServiceResponse.Transaction.ActiveSalesLines)
            {
                if (item.IsReturnLine())
                {
                    continue;
                }
                else
                {
                    capOnProductAndProductCategory = GetMarginCapOnProductAndProductCategory(request, item.ItemId, await GetProductIdAsync(request, item));
                    isMarginCapEnabledOnProductAndProductCategory = Convert.ToBoolean(Convert.ToInt32(capOnProductAndProductCategory?.GetProperty("ISMARGINCAPALLOWED")?.ToString() ?? decimal.Zero.ToString()));
                    excludeDiscount = Convert.ToBoolean(Convert.ToInt32(capOnProductAndProductCategory?.GetProperty("EXCLUDEDISCOUNT")?.ToString() ?? decimal.Zero.ToString()));
                    marginCapPercentageOnProductAndProductCategory = Convert.ToDecimal(capOnProductAndProductCategory?.GetProperty("MARGINCAPPERCENTAGE")?.ToString() ?? decimal.Zero.ToString());


                    if (excludeDiscount)
                    {
                        item.DiscountLines.Clear();
                        item.DiscountAmount = 0;
                        item.DiscountAmountWithoutTax = 0;
                        item.PeriodicDiscount = 0;
                        item.PeriodicPercentageDiscount = 0;
                    }
                    if (isMarginCapEnabledOnProductAndProductCategory)
                    {
                        if (marginCapPercentageOnProductAndProductCategory == 0)
                        {
                            marginCapPercentageOnProductAndProductCategory = marginCapPercentageOnStoreAndEntity;
                        }

                        decimal totalDiscountPercentageWithoutTenderDiscount = item.DiscountLines.Where(a => !offerIds.Contains(a.OfferId)).Sum(a => a.EffectivePercentage);

                        var costPrice = GetCostPrice(item, request);
                        var grossMargin = CalculateGrossMargin(costPrice, item?.Price ?? decimal.Zero);
                        grossMargin = Convert.ToDecimal(String.Format("{0:0.00}", grossMargin));

                        if ((grossMargin - marginCapPercentageOnProductAndProductCategory) <= totalDiscountPercentageWithoutTenderDiscount)
                        {
                            decimal newDiscount = grossMargin - marginCapPercentageOnProductAndProductCategory;
                            if (newDiscount < Decimal.Zero)
                            {
                                newDiscount = Decimal.Zero;
                            }
                            decimal numberOfLinesWithoutAskariDiscount = ((item.DiscountLines.Where(a => !offerIds.Contains(a.OfferId)).ToList().Count != (int)decimal.Zero) ? item.DiscountLines.Where(a => !offerIds.Contains(a.OfferId)).ToList().Count : decimal.One);
                            decimal newDistributedDiscount = newDiscount / numberOfLinesWithoutAskariDiscount;
                            foreach (var discountLine in item.DiscountLines)
                            {
                                if (!offerIds.Contains(discountLine.OfferId))
                                {
                                    discountLine.EffectivePercentage = newDistributedDiscount;
                                    discountLine.Percentage = newDistributedDiscount;
                                    discountLine.Amount = item.AgreementPrice * item.Quantity * (discountLine.EffectivePercentage / 100);
                                    discountLine.EffectiveAmount = item.AgreementPrice * item.Quantity * (discountLine.EffectivePercentage / 100);
                                }
                            }

                            foreach (var discountLine in item.DiscountLines.Where(a => offerIds.Contains(a.OfferId)).ToList())
                            {
                                discountLine.EffectiveAmount = item.AgreementPrice * item.Quantity * (discountLine.EffectivePercentage / 100);
                            }

                            item.TenderDiscountAmount = item.DiscountLines.Where(dl => dl.DiscountLineType == DiscountLineType.TenderTypeDiscount).Sum(dl => dl.EffectiveAmount);
                            item.DiscountAmount = item.DiscountLines.Sum(a => a.EffectiveAmount);
                            item.DiscountAmountWithoutTax = item.DiscountAmount;
                            item.PeriodicDiscount = item.DiscountAmount;
                            item.PeriodicPercentageDiscount = newDiscount;
                            item.TotalPercentageDiscount = newDiscount;
                        }
                    }
                }
            }
            getPriceServiceResponse.Transaction.DiscountAmount = getPriceServiceResponse.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);
            getPriceServiceResponse.Transaction.DiscountAmountWithoutTax = getPriceServiceResponse.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);
            getPriceServiceResponse.Transaction.PeriodicDiscountAmount = getPriceServiceResponse.Transaction.ActiveSalesLines.Sum(a => a.DiscountAmount);

            CalculateTaxServiceRequest calculateTax = new CalculateTaxServiceRequest(getPriceServiceResponse.Transaction);
            return new GetPriceServiceResponse(calculateTax.Transaction);
        }
        public GetMarginCapOnStoreAndLoyaltyProgram GetMarginCapOnStoreAndLoyaltyProgram(Request request)
        {
            CalculateDiscountsServiceRequest discountsServiceRequest = (CalculateDiscountsServiceRequest)request;
            ParameterSet parameters = new ParameterSet();
            parameters["@LoyaltyId"] = discountsServiceRequest.Transaction.AffiliationLoyaltyTierLines.Where(a => a.AffiliationType == RetailAffiliationType.Loyalty).FirstOrDefault().AffiliationId;
            parameters["@StoreNumber"] = request.RequestContext.GetDeviceConfiguration().StoreNumber;
            parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var result = databaseContext.ExecuteStoredProcedure<GetMarginCapOnStoreAndLoyaltyProgram>("ext.GETMARGINCAPONLOYALTYANDSTORE", parameters, QueryResultSettings.AllRecords);
                return result.FirstOrDefault();
            }
        }

        public GetMarginCapOnProductAndProductCategory GetMarginCapOnProductAndProductCategory(Request request, String itemId, long productId)
        {
            CalculateDiscountsServiceRequest discountsServiceRequest = (CalculateDiscountsServiceRequest)request;
            ParameterSet parameters = new ParameterSet();
            parameters["@ProductID"] = productId;
            parameters["@ItemId"] = itemId;
            parameters["@StoreNumber"] = request.RequestContext.GetDeviceConfiguration().StoreNumber;
            parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var result = databaseContext.ExecuteStoredProcedure<GetMarginCapOnProductAndProductCategory>("ext.GETMARGINCAPONPRODUCTANDPRODUCTCATEGORY", parameters, QueryResultSettings.AllRecords);
                return result.FirstOrDefault();
            }
        }
        public async Task<long> GetProductIdAsync(Request request, SalesLine item)
        {
            ProductSearchCriteria searchCriteria = new ProductSearchCriteria(request.RequestContext.GetDeviceConfiguration().ChannelId);
            searchCriteria.Ids.Add(item.ProductId);
            GetProductServiceRequest productServiceRequest = new GetProductServiceRequest(searchCriteria, request.RequestContext.LanguageId, false, QueryResultSettings.AllRecords);
            productServiceRequest.RequestContext = request.RequestContext;
            productServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
            ProductSearchServiceResponse result = (ProductSearchServiceResponse)await ExecuteBaseRequestAsync(productServiceRequest);
            return result.ProductSearchResult.Results.FirstOrDefault().RecordId;
        }
        public decimal CalculateGrossMargin(decimal costPrice, decimal sellPrice)
        {
            return ((sellPrice - costPrice) / sellPrice) * 100;
        }

        public decimal GetCostPrice(SalesLine item, Request request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                {
                    DatabaseSchema = "ext",
                    Select = new ColumnSet("CostPrice"),
                    From = "CDCPRODUCTVARIANTCOSTPRICE",
                    Where = "DATAAREAID = @dataAreaId AND ITEMID = @itemId AND CONFIGID = @configId AND INVENTLOCATIONID = @inventLocationId AND INVENTCOLORID = @inventColorId AND INVENTSTYLEID = @inventStyleId AND INVENTSIZEID = @inventSizeId",
                    OrderBy = "RECID"
                };

                query.Parameters["@dataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@itemId"] = item.ItemId;
                query.Parameters["@inventLocationId"] = item.InventoryLocationId ?? string.Empty;
                query.Parameters["@inventStyleId"] = item.Variant.StyleId ?? string.Empty;
                query.Parameters["@inventColorId"] = item.Variant.ColorId ?? string.Empty;
                query.Parameters["@inventSizeId"] = item.Variant.SizeId ?? string.Empty;
                query.Parameters["@configId"] = item.Variant?.ConfigId ?? string.Empty;

                var itemCostPrice = databaseContext.ReadEntity<ItemCostPrice>(query);
                if (!itemCostPrice.Results.IsNullOrEmpty())
                {
                    decimal costPrice = Convert.ToDecimal(Convert.ToString(itemCostPrice.FirstOrDefault().GetProperty("COSTPRICE")));
                    return costPrice;
                }
                else
                {
                    return 0.00M;
                }
            }
        }
        public async Task<Response> ExecuteBaseRequestAsync(Request request)
        {
            var requestHandler = request.RequestContext.Runtime.GetNextAsyncRequestHandler(request.GetType(), this);
            Response response = await request.RequestContext.Runtime.ExecuteAsync<Response>(request, request.RequestContext, requestHandler, false).ConfigureAwait(false);
            return response;
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


    }
}