namespace CDC.Commerce.Runtime.FractionalSale
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CDC.Commerce.Runtime.FractionalSale.Entities;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using System.Linq;
    using Newtonsoft.Json;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

    public class FractionalSaleDataService : IRequestHandlerAsync
    {
        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(FractionSaleRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request is FractionSaleRequest)
            {
                return await ValidateFractionalSale((FractionSaleRequest)request);
            }
            else
            {
                throw new CommerceException("Retail Server", "Request type not supported.");
            }

        }

        private async Task<FractionSaleResponse> ValidateFractionalSale(FractionSaleRequest request)
        {
            var response = new FractionSaleResponse(true, string.Empty);
            try
            {
                ChannelConfiguration channelConfigs = request.RequestContext.GetChannelConfiguration();
                if (request != null)
                {
                    var UnitNotAllowedForFractionalSale = GetRetailConfigurationParameter(request, "UnitNotAllowedForFractionalSale", channelConfigs.InventLocationDataAreaId);
                    if (string.IsNullOrWhiteSpace(UnitNotAllowedForFractionalSale))
                    {
                        return new FractionSaleResponse(true, "");
                    }
                    else
                    {
                        var factionalSalesUnits = UnitNotAllowedForFractionalSale.ToLower().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var fractionsalSaleProducts = await GetFractionalSaleProducts(channelConfigs.InventLocation, request);
                        foreach (var product in request.ProductsInformation)
                        {
                            if (factionalSalesUnits.Any(x => x == product.UnitOfMeasure?.ToLower()))
                            {
                                var isProductExists = IsFractionalSaleProduct(request, channelConfigs.InventLocation, product.ProductId, fractionsalSaleProducts);
                                if (!isProductExists)
                                {
                                    response = new FractionSaleResponse(false, "");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CommerceException("Retail Server", exception.Message);
            }

            return response;
        }

        private string GetRetailConfigurationParameter(Request request, string name, string company)
        {
            var configurationRequest = new GetConfigurationParametersDataRequest(request.RequestContext.GetChannelConfiguration().RecordId);
            var configurationResponse = request.RequestContext.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string result = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (name).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;
            return result;
        }

        private bool IsFractionalSaleProduct(Request request, string storeNumber, long productRecId, List<CDCProductFractionalSale> cDCProductFractionalSales)
        {
            #region Check by Variant Id
            var variants = cDCProductFractionalSales.Where(x => x.VARIANT != 0).ToList();
            if (variants.Any(y => y.VARIANT == productRecId))
            {
                return true;
            }
            #endregion
            

            #region Check by Product
            var productIds = cDCProductFractionalSales.Where(x => x.VARIANT == 0 && x.PRODUCT != 0).Select(x=>x.PRODUCT).ToList();

            List<ExtensionsEntity> entities;
            foreach (var productId in productIds)
            {
                GetAllProductVariantsByMasterProductId(productId, request.RequestContext, out entities);
                var productVariants = GetProductVariants(entities, request.RequestContext).Result.AsEnumerable();
                if(productVariants.Any(x=>x.DistinctProductVariantId == productRecId))
                {
                    return true;
                }
            }
            List<ItemVariantInventoryDimension> products = new List<ItemVariantInventoryDimension>();

            #endregion

            #region Check by Categories 
            var categoryIds = cDCProductFractionalSales.Where(x => x.VARIANT == 0 && x.PRODUCT == 0 && x.CATEGORY != 0).Select(x => x.CATEGORY).ToList();
            foreach(var categoryId in categoryIds)
            {
                var checkInCategoryDataRequest = new CheckIfProductOrVariantAreInCategoryDataRequest(productRecId, categoryId);
                var checkInCategoryDataResponse = request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(checkInCategoryDataRequest).Result;
                if (checkInCategoryDataResponse.Entity)
                {
                    return true;
                }
            }
            #endregion

            return false;
        }


        private async Task<List<CDCProductFractionalSale>> GetFractionalSaleProducts(string storeNumber, Request request)
        {
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                var query  = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    DatabaseSchema = "EXT",
                    Select = new ColumnSet("*"),
                    From = "CDCPRODUCTFRACTIONALSALE",
                    Where = "STORENUMBER = @storeNumber "
                };
                query.Parameters["@storeNumber"] = storeNumber;

                var cdcProductFractionalSale = await databaseContext.ReadEntityAsync<CDCProductFractionalSale>(query).ConfigureAwait(false);
                if (cdcProductFractionalSale.FirstOrDefault() != null)
                {
                    return cdcProductFractionalSale.AsEnumerable().ToList();
                }
                else
                {
                    return new List<CDCProductFractionalSale>();
                }
            }
        }

        public void GetAllProductVariantsByMasterProductId(long masterProductId, RequestContext context, out List<ExtensionsEntity> entities)
        {
            if (masterProductId.ToString() == null || string.IsNullOrEmpty(masterProductId.ToString()))
            {
                entities = new List<ExtensionsEntity>();
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();
                query.QueryString = $@"Select I1.RETAILVARIANTID, I1.DISTINCTPRODUCTVARIANT, I1.INVENTDIMID, I1.ITEMID from ax.ECORESDISTINCTPRODUCTVARIANT E1 JOIN ax.INVENTDIMCOMBINATION I1 on I1.DISTINCTPRODUCTVARIANT = E1.RECID WHERE  E1.PRODUCTMASTER = @masterProductId AND DATAAREAID = @dataAreaId";
                query.Parameters["@masterProductId"] = masterProductId;
                query.Parameters["@dataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;

                try
                {
                    entities = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList();
                }
                catch (Exception)
                {
                    entities = new List<ExtensionsEntity>();
                }
            }
        }
        public async Task<EntityDataServiceResponse<ProductVariant>> GetProductVariants(List<ExtensionsEntity> entities, RequestContext context)
        {
            PrepareProductVariantsData(entities, out List<ItemVariantInventoryDimension> itemVariantInventoryDimension);
            GetProductVariantsDataRequest getProductVariantsDataRequest = new GetProductVariantsDataRequest(itemVariantInventoryDimension)
            {
                RequestContext = context
            };
            return await context.Runtime.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(getProductVariantsDataRequest, context);
        }

        public void PrepareProductVariantsData(List<ExtensionsEntity> entities, out List<ItemVariantInventoryDimension> itemVariantInventoryDimension)
        {
            itemVariantInventoryDimension = new List<ItemVariantInventoryDimension>();
            foreach (var item in entities)
            {
                itemVariantInventoryDimension.Add(new ItemVariantInventoryDimension(item.GetProperty("ITEMID")?.ToString() ?? string.Empty, item.GetProperty("INVENTDIMID")?.ToString() ?? string.Empty));
            }
        }
    }
}
