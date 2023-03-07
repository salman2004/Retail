
namespace CDC.Commerce.Runtime.ProductSearch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Data;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Framework;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    using Microsoft.Dynamics.Retail.Diagnostics;

    public class ProductVariantInventoryRequestHandler : IRequestHandlerAsync
    {

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                      typeof(GetProductDimensionValuesDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            EntityDataServiceResponse<ProductDimensionValue> response = await this.ExecuteNextAsync<EntityDataServiceResponse<ProductDimensionValue>>(request);
            response =  await FilterproductDimensionOnInventoryAsync((GetProductDimensionValuesDataRequest)request, response);
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="productDimensionValues"></param>
        public async Task<EntityDataServiceResponse<ProductDimensionValue>> FilterproductDimensionOnInventoryAsync(GetProductDimensionValuesDataRequest request, EntityDataServiceResponse<ProductDimensionValue> productDimensionValues)
        {
            GetConfigurationParameters(request.RequestContext, "FilterProductsOnInventory", out string isProductFilterAllowed);
            if (Convert.ToBoolean(isProductFilterAllowed?? "false"))
            {
                GetAllProductVariantsByMasterProductId(request.MasterProductId, request.RequestContext, out List<ExtensionsEntity> productVariantEntities);
                if (productDimensionValues != null)
                {
                    EntityDataServiceResponse<ProductVariant> productVariants = await GetProductVariants(productVariantEntities, request.RequestContext);
                    GetEstimatedProductWarehouseAvailabilityServiceResponse availabilityServiceResponse = await GetEstimatedProductWarehouseAvailabilityAsync(productVariantEntities, request.RequestContext);
                    List<ProductDimensionValue> productDimensionValuesFiltered = new List<ProductDimensionValue>();
                    if (!productDimensionValues.IsNullOrEmpty())
                    {
                        foreach (var item in productDimensionValues)
                        {
                            List<ProductVariant> productVariantsTmp = productVariants.Where(pv => pv.ColorId == item.DimensionId || pv.ConfigId == item.DimensionId || pv.SizeId == item.DimensionId || pv.StyleId == item.DimensionId).ToList();
                            if (!request.MatchingDimensionValues.IsNullOrEmpty())
                            {
                                if (!request.MatchingDimensionValues.Where(a => a.DimensionType == ProductDimensionType.Color).IsNullOrEmpty())
                                {
                                    productVariantsTmp = productVariantsTmp.Where(pvt => pvt.ColorId == Convert.ToString(request.MatchingDimensionValues?.Where(a => a.DimensionType == ProductDimensionType.Color)?.FirstOrDefault()?.DimensionValue.Value ?? string.Empty)).ToList();
                                }
                                if (!request.MatchingDimensionValues.Where(a => a.DimensionType == ProductDimensionType.Size).IsNullOrEmpty())
                                {
                                    productVariantsTmp = productVariantsTmp.Where(pvt => pvt.Size == Convert.ToString(request.MatchingDimensionValues?.Where(a => a.DimensionType == ProductDimensionType.Size)?.FirstOrDefault()?.DimensionValue.Value ?? string.Empty)).ToList();
                                }
                                if (!request.MatchingDimensionValues.Where(a => a.DimensionType == ProductDimensionType.Style).IsNullOrEmpty())
                                {
                                    productVariantsTmp = productVariantsTmp.Where(pvt => pvt.StyleId == Convert.ToString(request.MatchingDimensionValues?.Where(a => a.DimensionType == ProductDimensionType.Style)?.FirstOrDefault()?.DimensionValue.Value ?? string.Empty)).ToList();
                                }
                                if (!request.MatchingDimensionValues.Where(a => a.DimensionType == ProductDimensionType.Configuration).IsNullOrEmpty())
                                {
                                    productVariantsTmp = productVariantsTmp.Where(pvt => pvt.ConfigId == Convert.ToString(request.MatchingDimensionValues?.Where(a => a.DimensionType == ProductDimensionType.Configuration)?.FirstOrDefault()?.DimensionValue.Value ?? string.Empty)).ToList();
                                }
                            }
                            if (availabilityServiceResponse.ProductWarehouseInventoryInformation.ProductWarehouseInventoryAvailabilities.Where(a => productVariantsTmp.Any(b => b.DistinctProductVariantId == a.ProductId)).Any(ph => ph.PhysicalAvailable > 0))
                            {
                                productDimensionValuesFiltered.Add(item);
                            }
                        }
                        return new EntityDataServiceResponse<ProductDimensionValue>(productDimensionValuesFiltered.AsPagedResult());
                    }
                    
                }
            }
           
            return productDimensionValues;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configName"></param>
        /// <param name="result"></param>
        public void GetConfigurationParameters(RequestContext context, string configName, out string result)
        {
            result = string.Empty;

            // Get the configuration parameters
            var configurationRequest = new GetConfigurationParametersDataRequest(context.GetChannelConfiguration().RecordId);
            var configurationResponse = context.ExecuteAsync<EntityDataServiceResponse<RetailConfigurationParameter>>(configurationRequest).Result;

            string value = configurationResponse?.PagedEntityCollection?.Where(cp => string.Equals(cp.Name.ToUpper().Trim(), (configName).ToUpper().Trim(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<EntityDataServiceResponse<ProductVariant>> GetProductVariants(List<ExtensionsEntity> entities, RequestContext context)
        {
            PrepareProductVariantsData(entities, out List<ItemVariantInventoryDimension> itemVariantInventoryDimension);
            GetProductVariantsDataRequest getProductVariantsDataRequest = new GetProductVariantsDataRequest(itemVariantInventoryDimension)
            {
                RequestContext = context
            };
            return await context.Runtime.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(getProductVariantsDataRequest, context);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PrepareProductVariantsData(List<ExtensionsEntity> entities, out List<ItemVariantInventoryDimension> itemVariantInventoryDimension)
        {
            itemVariantInventoryDimension = new List<ItemVariantInventoryDimension>();
            foreach (var item in entities)
            {
                itemVariantInventoryDimension.Add(new ItemVariantInventoryDimension(item.GetProperty("ITEMID")?.ToString() ?? string.Empty, item.GetProperty("INVENTDIMID")?.ToString() ?? string.Empty));
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productVariantEntities"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<GetEstimatedProductWarehouseAvailabilityServiceResponse> GetEstimatedProductWarehouseAvailabilityAsync(List<ExtensionsEntity> productVariantEntities, RequestContext context)
        {
            PrepareListProductWarehousr(productVariantEntities, context, out List<ProductWarehouse> productWarehouses);
            GetEstimatedProductWarehouseAvailabilityServiceRequest getProductDimensionsInventoryAvailabilityDataRequest = new GetEstimatedProductWarehouseAvailabilityServiceRequest(productWarehouses)
            {
                RequestContext = context
            };
            return await context.Runtime.ExecuteAsync<GetEstimatedProductWarehouseAvailabilityServiceResponse>(getProductDimensionsInventoryAvailabilityDataRequest, context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productVariants"></param>
        /// <param name="context"></param>
        /// <param name="productWarehouses"></param>
        public void PrepareListProductWarehousr(List<ExtensionsEntity> productVariantEntities, RequestContext context, out List<ProductWarehouse> productWarehouses)
        {
            productWarehouses = new List<ProductWarehouse>();
            foreach (var item in productVariantEntities)
            {
                productWarehouses.Add(new ProductWarehouse(Convert.ToInt64(item.GetProperty("DISTINCTPRODUCTVARIANT")?.ToString() ?? decimal.Zero.ToString()), context.GetChannelConfiguration().InventLocation, context.GetChannelConfiguration().InventLocationDataAreaId));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="masterProductId"></param>
        /// <param name="context"></param>
        /// <param name="entities"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productVariants"></param>
        /// <param name="context"></param>
        /// <param name="productWarehouses"></param>
        public void PrepareListProductWarehousr(EntityDataServiceResponse<ProductVariant> productVariants, RequestContext context, out List<ProductWarehouse> productWarehouses)
        {
            productWarehouses = new List<ProductWarehouse>();
            foreach (var item in productVariants)
            {
                productWarehouses.Add(new ProductWarehouse(item.DistinctProductVariantId, context.GetChannelConfiguration().InventLocation, context.GetChannelConfiguration().InventLocationDataAreaId));
            }
        }
    }
}
